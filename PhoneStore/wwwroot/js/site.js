document.addEventListener('DOMContentLoaded', function () {

    function showToast(message, isError = false) {
        const toastContainer = document.getElementById('toast-container');
        if (!toastContainer) return;

        const toast = document.createElement('div');
        toast.className = `toast align-items-center text-white ${isError ? 'bg-danger' : 'bg-success'} border-0 show`;
        toast.role = 'alert';
        toast.ariaLive = 'assertive';
        toast.ariaAtomic = 'true';

        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body fs-6">
                    ${isError ? '<i class="fas fa-exclamation-circle me-2"></i>' : '<i class="fas fa-check-circle me-2"></i>'}
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        `;

        toastContainer.appendChild(toast);
        const bootstrapToast = new bootstrap.Toast(toast, { delay: 3000 });
        bootstrapToast.show();

        toast.addEventListener('hidden.bs.toast', () => toast.remove());
    }

    function updateCartIcon(count) {
        const cartLink = document.getElementById('cart-link');
        if (!cartLink) return;

        let countBadge = cartLink.querySelector('.badge-count');
        if (count > 0) {
            countBadge.textContent = count;
            countBadge.classList.remove('d-none');
        } else {
            countBadge.classList.add('d-none');
        }
    }

    function updateWishlistIcon(count) {
        const wishlistLink = document.getElementById('wishlist-link');
        if (!wishlistLink) return;

        let countBadge = wishlistLink.querySelector('.badge-count');
        if (count > 0) {
            countBadge.textContent = count;
            countBadge.classList.remove('d-none');
        } else {
            countBadge.classList.add('d-none');
        }
    }

    function getToken() {
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenElement ? tokenElement.value : '';
    }

    document.body.addEventListener('click', function (e) {
        const target = e.target.closest('.btn-add-to-cart');
        if (!target) return;

        e.preventDefault();
        const productId = target.dataset.id;

        const originalContent = target.innerHTML;
        target.disabled = true;
        target.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';

        fetch('/Cart/AddToCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getToken()
            },
            body: JSON.stringify({ Id: parseInt(productId), Quantity: 1 })
        })
            .then(res => {
                if (!res.ok) throw new Error('Network response was not ok');
                return res.json();
            })
            .then(data => {
                if (data.success) {
                    showToast(data.message);
                    updateCartIcon(data.count);
                } else {
                    showToast(data.message || "حدث خطأ", true);
                }
            })
            .catch(err => {
                console.error(err);
                showToast("حدث خطأ في الاتصال", true);
            })
            .finally(() => {
                target.disabled = false;
                target.innerHTML = originalContent;
            });
    });

    document.body.addEventListener('click', function (e) {
        const target = e.target.closest('.btn-toggle-wishlist');
        if (!target) return;

        e.preventDefault();
        const productId = target.dataset.id;
        const icon = target.querySelector('i');

        const isCurrentlyInWishlist = icon.classList.contains('fa-solid'); 
        const url = isCurrentlyInWishlist ? '/Cart/RemoveFromWishlist' : '/Cart/AddToWishlist';

        target.disabled = true;

        fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getToken() 
            },
            body: JSON.stringify({ Id: parseInt(productId) })
        })
            .then(res => {
                if (!res.ok) throw new Error('Network response was not ok');
                return res.json();
            })
            .then(data => {
                if (data.success) {
                    showToast(data.message);
                    if (data.count !== undefined) updateWishlistIcon(data.count);

                    if (isCurrentlyInWishlist) {
                        icon.classList.remove('fa-solid', 'text-danger');
                        icon.classList.add('fa-regular');
                        icon.style.color = '';
                    } else {
                        icon.classList.remove('fa-regular');
                        icon.classList.add('fa-solid', 'text-danger');
                        icon.style.color = 'red';
                    }
                } else {
                    showToast(data.message || "حدث خطأ", true);
                }
            })
            .catch(err => {
                console.error(err);
                showToast("حدث خطأ في الاتصال", true);
            })
            .finally(() => target.disabled = false);
    });

    document.body.addEventListener('click', function (e) {
        const target = e.target.closest('.btn-remove-from-wishlist-page');
        if (!target) return;

        e.preventDefault();
        const productId = target.dataset.id;
        const productCard = target.closest('.col-12');

        fetch('/Cart/RemoveFromWishlist', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getToken()
            },
            body: JSON.stringify({ Id: parseInt(productId) })
        })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    showToast(data.message);
                    if (data.count !== undefined) updateWishlistIcon(data.count);

                    if (productCard) {
                        productCard.remove();
                        const remainingItems = document.querySelectorAll('#wishlist-body .col-12');
                        if (remainingItems.length === 0) window.location.reload();
                    }
                } else {
                    showToast(data.message || "حدث خطأ", true);
                }
            })
            .catch(() => showToast("حدث خطأ في الاتصال", true));
    });

    document.body.addEventListener('click', function (e) {
        const target = e.target.closest('.btn-update-quantity');
        if (!target) return;

        e.preventDefault();
        const productId = target.dataset.id;
        const quantity = target.dataset.qty;
        if (quantity < 0) return;
        const tableBody = document.getElementById('cart-body');
        if (tableBody) tableBody.style.opacity = '0.5';

        fetch(`/Cart/UpdateQuantity?id=${productId}&quantity=${quantity}`, { method: 'POST' })
            .then(response => response.text())
            .then(html => {
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');
                const newCartSection = doc.getElementById('cart');
                if (newCartSection && document.getElementById('cart')) {
                    document.getElementById('cart').innerHTML = newCartSection.innerHTML;
                }
            })
            .catch(() => {
                showToast("حدث خطأ أثناء تحديث السلة", true);
                if (tableBody) tableBody.style.opacity = '1';
            });
    });

    document.body.addEventListener('click', function (e) {
        const target = e.target.closest('.btn-remove-from-cart');
        if (!target) return;
        e.preventDefault();
        const productId = target.dataset.id;

        fetch('/Cart/RemoveFromCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getToken()
            },
            body: JSON.stringify({ Id: parseInt(productId) })
        })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    showToast(data.message);
                    updateCartIcon(data.count);
                    window.location.reload();
                } else {
                    showToast(data.message || "حدث خطأ", true);
                }
            })
            .catch(() => showToast("حدث خطأ في الاتصال", true));
    });
});