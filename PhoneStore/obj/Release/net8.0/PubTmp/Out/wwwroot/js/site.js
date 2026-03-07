// الانتظار حتى يتم تحميل محتوى الصفحة بالكامل
document.addEventListener('DOMContentLoaded', function () {

    // --------------------------------------------------------
    // 1. دوال مساعدة (Helper Functions)
    // --------------------------------------------------------

    // دالة للحصول على رمز الحماية (Anti-Forgery Token) المطلوب لعمليات POST
    function getToken() {
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenElement ? tokenElement.value : '';
    }

    // دالة لعرض الإشعارات (Toasts) أسفل الشاشة
    window.showToast = function (message, isError = false) {
        const toastContainer = document.getElementById('toast-container');
        if (!toastContainer) return;

        const bgClass = isError ? 'bg-danger' : 'bg-success';
        const iconClass = isError ? 'fa-exclamation-circle' : 'fa-check-circle';

        const toastHtml = `
            <div class="toast align-items-center text-white ${bgClass} border-0 mb-2 shadow-lg" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body fw-bold" style="font-size: 1.1rem;">
                        <i class="fas ${iconClass} me-2"></i> ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-3 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;

        toastContainer.insertAdjacentHTML('beforeend', toastHtml);
        const toastElement = toastContainer.lastElementChild;
        const toast = new bootstrap.Toast(toastElement, { delay: 3500 });
        toast.show();

        toastElement.addEventListener('hidden.bs.toast', () => {
            toastElement.remove();
        });
    };

    // دالة لتحديث الأرقام الحمراء (Badges) فوق أيقونات السلة والمفضلة
    function updateBadge(selector, count) {
        const badge = document.querySelector(selector);
        if (badge) {
            badge.textContent = count;
            if (count > 0) {
                badge.classList.remove('d-none');
            } else {
                badge.classList.add('d-none');
            }
        }
    }

    // --------------------------------------------------------
    // 2. إدارة سلة التسوق (Cart Management)
    // --------------------------------------------------------

    // الإضافة للسلة من صفحة التفاصيل (مع قراءة اللون والنوع)
    document.body.addEventListener('click', function (e) {
        const target = e.target.closest('.btn-add-to-cart-detailed');
        if (!target) return;

        e.preventDefault();
        const productId = target.dataset.id;

        // جلب الخيارات المختارة إن وجدت
        const colorSelect = document.getElementById('selectedColor');
        const typeSelect = document.getElementById('selectedType');

        const selectedColor = colorSelect ? colorSelect.value : null;
        const selectedType = typeSelect ? typeSelect.value : null;

        const originalContent = target.innerHTML;
        target.disabled = true;
        target.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i> جاري الإضافة...';

        fetch('/Cart/AddToCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getToken()
            },
            body: JSON.stringify({
                Id: parseInt(productId),
                Quantity: 1,
                Color: selectedColor,
                Type: selectedType
            })
        })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    showToast(data.message);
                    updateBadge('#cart-link .badge-count', data.count);
                } else {
                    showToast(data.message || "حدث خطأ أثناء الإضافة", true);
                }
            })
            .catch(err => {
                console.error(err);
                showToast("حدث خطأ في الاتصال بالخادم", true);
            })
            .finally(() => {
                target.disabled = false;
                target.innerHTML = originalContent;
            });
    });

    // الإضافة السريعة للسلة (من صفحة عرض المنتجات أو المفضلة)
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
            body: JSON.stringify({
                Id: parseInt(productId),
                Quantity: 1
            })
        })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    showToast(data.message);
                    updateBadge('#cart-link .badge-count', data.count);
                } else {
                    showToast(data.message || "حدث خطأ", true);
                }
            })
            .catch(() => showToast("حدث خطأ في الاتصال", true))
            .finally(() => {
                target.disabled = false;
                target.innerHTML = originalContent;
            });
    });

    // تغيير كمية منتج داخل صفحة السلة
    document.body.addEventListener('click', function (e) {
        const target = e.target.closest('.btn-update-quantity');
        if (!target) return;

        e.preventDefault();
        const productId = target.dataset.id;
        const newQty = target.dataset.qty;

        if (newQty < 1) return; // منع تقليل الكمية لأقل من 1

        target.disabled = true;

        const formData = new FormData();
        formData.append('id', productId);
        formData.append('quantity', newQty);

        fetch('/Cart/UpdateQuantity', {
            method: 'POST',
            headers: {
                'RequestVerificationToken': getToken()
            },
            body: formData
        })
            .then(res => {
                if (res.ok) {
                    // إعادة تحميل الصفحة لتحديث الإجماليات بشكل دقيق
                    location.reload();
                } else {
                    showToast("حدث خطأ أثناء تحديث الكمية", true);
                    target.disabled = false;
                }
            })
            .catch(() => {
                showToast("حدث خطأ في الاتصال", true);
                target.disabled = false;
            });
    });

    // حذف منتج بالكامل من السلة
    document.body.addEventListener('click', function (e) {
        const target = e.target.closest('.btn-remove-from-cart');
        if (!target) return;

        e.preventDefault();
        const productId = target.dataset.id;

        if (!confirm('هل أنت متأكد من حذف هذا المنتج من السلة؟')) return;

        target.disabled = true;

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
                    location.reload();
                } else {
                    showToast(data.message || "حدث خطأ", true);
                    target.disabled = false;
                }
            })
            .catch(() => {
                showToast("حدث خطأ في الاتصال", true);
                target.disabled = false;
            });
    });

    // --------------------------------------------------------
    // 3. إدارة المفضلة (Wishlist Management)
    // --------------------------------------------------------

    // التبديل (إضافة / إزالة) للمفضلة من صفحة المتجر والتفاصيل
    document.body.addEventListener('click', function (e) {
        const target = e.target.closest('.btn-toggle-wishlist');
        if (!target) return;

        e.preventDefault();
        const productId = target.dataset.id;

        // فحص ما إذا كان المنتج في المفضلة بناءً على الأيقونة الحالية
        const isCurrentlyInWishlist = target.querySelector('.fa-solid.fa-heart') !== null;
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
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    showToast(data.message);
                    updateBadge('#wishlist-link .badge-count', data.count);

                    // تحديث شكل الزر فوراً دون إعادة تحميل
                    if (isCurrentlyInWishlist) {
                        target.innerHTML = '<i class="fa-regular fa-heart fs-4"></i>';
                        target.classList.remove('btn-purple-custom', 'active', 'text-danger');
                        target.classList.add('btn-outline-purple-custom');
                        target.title = "إضافة للمفضلة";
                    } else {
                        target.innerHTML = '<i class="fa-solid fa-heart fs-4"></i>';
                        target.classList.remove('btn-outline-purple-custom');
                        target.classList.add('btn-purple-custom', 'active');
                        target.title = "إزالة من المفضلة";
                    }
                } else {
                    showToast(data.message || "حدث خطأ", true);
                }
            })
            .catch(() => showToast("حدث خطأ في الاتصال", true))
            .finally(() => {
                target.disabled = false;
            });
    });

    // إزالة منتج من صفحة المفضلة نفسها (Wishlist Page)
    document.body.addEventListener('click', function (e) {
        const target = e.target.closest('.btn-remove-from-wishlist-page');
        if (!target) return;

        e.preventDefault();
        const productId = target.dataset.id;
        target.disabled = true;

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
                    updateBadge('#wishlist-link .badge-count', data.count);

                    // حذف بطاقة المنتج من واجهة المستخدم
                    const card = target.closest('.col-12.col-sm-6');
                    if (card) {
                        card.remove();
                    }

                    // إذا أصبحت المفضلة فارغة، أعد تحميل الصفحة لإظهار رسالة "المفضلة فارغة"
                    if (data.count === 0) {
                        location.reload();
                    }
                }
            })
            .catch(() => {
                showToast("حدث خطأ في الاتصال", true);
                target.disabled = false;
            });
    });

});