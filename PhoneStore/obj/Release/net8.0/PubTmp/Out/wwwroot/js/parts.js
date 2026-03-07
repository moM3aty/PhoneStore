const data = {
    apple: {
        "شاشات": ["شاشة iPhone 11", "شاشة iPhone 12", "شاشة iPhone 13"],
        "بطاريات": ["بطارية iPhone XR", "بطارية iPhone 8"],
        "بوردة": ["بوردة iPhone 7", "بوردة iPhone 11"]
    },
    samsung: {
        "شاشات": ["شاشة S20", "شاشة S22", "شاشة Note 10"],
        "بطاريات": ["بطارية A50", "بطارية S8"],
        "كاميرات": ["كاميرا J7", "كاميرا A30"]
    },
    xiaomi: {
        "شاشات": ["شاشة Redmi 9", "شاشة Note 11"],
        "بطاريات": ["بطارية Redmi 8", "بطارية Poco F3"]
    },
    huawei: {
        "شاشات": ["شاشة P30", "شاشة Mate 20"],
        "بطاريات": ["بطارية Y7", "بطارية Nova 3"]
    }
};

function showParts(brand, btnElement) {
    const container = document.getElementById("parts-container");
    container.innerHTML = "";

    document.querySelectorAll(".brand-btn").forEach(btn => btn.classList.remove("active"));
    if (btnElement) btnElement.classList.add("active");

    const parts = data[brand];
    let firstPart = null;

    for (let partName in parts) {
        if (!firstPart) firstPart = partName;

        const div = document.createElement("div");
        div.classList.add("parts-list");
        div.innerHTML = `<div class="part-item" onclick="showProducts('${brand}', '${partName}', this)">${partName}</div>`;
        container.appendChild(div);
    }

    if (firstPart) {
        const firstPartDiv = container.querySelector(".part-item");
        showProducts(brand, firstPart, firstPartDiv);
    }
}

function showProducts(brand, partName, itemElement) {
    const container = document.getElementById("products-container");
    container.innerHTML = "";
    document.querySelectorAll(".part-item").forEach(el => el.classList.remove("active"));
    if (itemElement) itemElement.classList.add("active");

    data[brand][partName].forEach(item => {
        container.innerHTML += `
      <div class="col-12 col-md-6 col-lg-4">
        <div class="product-card position-relative overflow-hidden">
            <div class="wishlist position-absolute top-0 start-0 z-2">
               <button>
                  <i class="fa-regular fa-heart fa-lg" title="Wishlist"></i>
               </button>
            </div>
                <div class="product-img">
                    <img src="../images/iphone.png" alt="">
                </div>
         <div class="product-info">
          <h3 class="title">${item}</h3>
          <div>
           <span class="kind">Apple</span>
          </div>
          <p class="text-muted">قطعة غيار أصلية بجودة ممتازة</p>
          <div class="d-flex justify-content-between align-items-center">
                <div class="price">
                   <p>1200 ج.م</p>
                </div>
                <div><button class="btn btn-success btn-sm">اضف للسلة</button></div>
          </div>
         </div>
        </div>
      </div>
    `;
    });
}

document.addEventListener("DOMContentLoaded", function () {
    const firstBrandBtn = document.querySelector(".brand-btn");
    if (firstBrandBtn) {
        const brand = firstBrandBtn.textContent.toLowerCase();
        showParts(brand, firstBrandBtn);
    }
});
