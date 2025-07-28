$(function () {
    $(".book-image").on("click", function () {
        const bookName = $(this).parent().parent().find("h5").text()

        $("#box_search").val(bookName)

        $("#box_status").val("1")
        $("#btn_submit1").trigger("click")
    })

    $(".borrow-book-btn").on("click", function () {
        const bookName = $(this).parent().parent().find("h5").text()

        $("#box_search").val(bookName)

        $("#box_status").val("1")
        $("#btn_submit1").trigger("click")
    })

    $(".activity-img").on("click", function () {
        const activityName = $(this).parent().find("h5").text()
    })

    // 抓取 DOM 元素
    const carouselSpinner = document.getElementById('carouselSpinner');
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');
    const items = document.querySelectorAll('.my-carousel-item');
    const totalItems = items.length; // 輪播項目總數
    const rotateAngle = 360 / totalItems; // 每個項目之間的旋轉角度

    let currentRotation = 0; // 當前輪播的總旋轉角度

    // 初始化輪播的位置 (可以選擇讓它一開始就轉到某個位置)
    // carouselSpinner.style.transform = `translateY(20vh) translateZ(-500px) rotateY(${currentRotation}deg)`;

    // 處理「上一張」按鈕點擊事件
    prevBtn.addEventListener('click', () => {
        currentRotation += rotateAngle; // 每次旋轉一個項目的角度
        carouselSpinner.style.transform = `translateZ(-500px) rotateY(${currentRotation}deg)`;
    });

    // 處理「下一張」按鈕點擊事件
    nextBtn.addEventListener('click', () => {
        currentRotation -= rotateAngle; // 每次旋轉一個項目的角度
        carouselSpinner.style.transform = `translateZ(-500px) rotateY(${currentRotation}deg)`;
    });

    // 監聽鍵盤左右箭頭
    document.addEventListener('keydown', (event) => {
        if (event.key === 'ArrowLeft') {
            prevBtn.click(); // 模擬點擊上一張按鈕
        } else if (event.key === 'ArrowRight') {
            nextBtn.click(); // 模擬點擊下一張按鈕
        }
    });
})