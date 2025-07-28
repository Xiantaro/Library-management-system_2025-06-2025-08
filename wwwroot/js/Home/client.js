function switchToTab(tabId) {
    const trigger = document.querySelector(`[data-bs-target="#${tabId}"]`);

    if (trigger) {
        const tab = new bootstrap.Tab(trigger);

        tab.show()
    } else {
        console.warn(`找不到 data-bs-target="#${tabId}" 的 tab 控制元件`);
    }
}

//------------------------------- 評論與星數-------------------------------
//評論星星
document.querySelectorAll('.star-rating i').forEach(star => {
    // 懸停效果
    star.addEventListener('mouseover', function () {
        const rating = parseInt(this.getAttribute('data-rating'));
        highlightStars(rating);
    });

    // 點擊選擇
    star.addEventListener('click', function () {
        const rating = parseInt(this.getAttribute('data-rating'));
        document.getElementById('selectedRating').value = rating;
        setSelectedStars(rating);
    });
})

// 重置懸停效果
document.querySelector('.star-rating').addEventListener('mouseleave', function () {
    const selectedRating = parseInt(document.getElementById('selectedRating').value);
    if (selectedRating > 0) {
        setSelectedStars(selectedRating);
    } else {
        resetStars();
    }
})

// 高亮星星（懸停時）
function highlightStars(rating) {
    const stars = document.querySelectorAll('.star-rating i');
    stars.forEach((star, index) => {
        if (index < rating) {
            star.classList.remove('bi-star');
            star.classList.add('bi-star-fill');
        } else {
            star.classList.remove('bi-star-fill');
            star.classList.add('bi-star');
        }
    });
}

// 設定選中的星星（點擊後）
function setSelectedStars(rating) {
    const stars = document.querySelectorAll('.star-rating i');
    stars.forEach((star, index) => {
        if (index < rating) {
            star.classList.remove('bi-star');
            star.classList.add('bi-star-fill');
        } else {
            star.classList.remove('bi-star-fill');
            star.classList.add('bi-star');
        }
    });
}

// 重置星星狀態
function resetStars() {
    const stars = document.querySelectorAll('.star-rating i');
    stars.forEach(star => {
        star.classList.remove('bi-star-fill');
        star.classList.add('bi-star');
    });
}

//評論框字數實時變動
const commentText = document.getElementById('commentText');
const charCount = document.getElementById('charCount');

commentText.addEventListener('input', function () {
    const currentLength = this.value.length;
    charCount.textContent = currentLength;

    // 超過限制時截斷文字（雙重保障）
    if (currentLength > 200) {
        this.value = this.value.substring(0, 200);
        charCount.textContent = 200;
    }

    // 超過時變紅色提示
    charCount.style.color = currentLength >= 200 ? 'red' : '';
})

$(() => {
    $("#editPasswordBtn").on("click", () => {
        $("#editPasswordBtn").addClass("d-none")
        $("#confirmPasswordBtn").removeClass("d-none")
        $("#cancelPasswordBtn").removeClass("d-none")

        $("#passwordDisplay").addClass("d-none")
        $("#passwordInput1").removeClass("d-none")
    })

    $("#confirmPasswordBtn").on("click", () => {
        if ($("#passwordInput1").val() != "") {
            alert("變更未確認，再次輸入密碼")

            $("#confirmPasswordBtn").addClass("d-none")
            $("#savePasswordBtn").removeClass("d-none")

            $("#passwordInput1").addClass("d-none")
            $("#passwordInput2").removeClass("d-none")
        }
    })

    $("#savePasswordBtn").on("click", () => {
        if ($("#passwordInput2").val() != "") {
            if ($("#passwordInput1").val() != $("#passwordInput2").val()) {
                alert("密碼錯誤，再次輸入密碼")

                $("#passwordInput2").val("")
            }
            else {
                alert("變更已儲存")

                $("#passwordInput3").val($("#passwordInput2").val())
                $("#btn_password").trigger("click")

                $("#editPasswordBtn").removeClass("d-none")
                $("#savePasswordBtn").addClass("d-none")
                $("#cancelPasswordBtn").addClass("d-none")

                $("#passwordDisplay").removeClass("d-none")
                $("#passwordInput2").addClass("d-none")
            }
        }
    })

    $("#cancelPasswordBtn").on("click", () => {
        if ($("#passwordInput1").val() != "" || $("#passwordInput2").val() != "") { alert("變更未儲存，確定取消?") }

        $("#editPasswordBtn").removeClass("d-none")
        $("#confirmPasswordBtn").addClass("d-none")
        $("#savePasswordBtn").addClass("d-none")
        $("#cancelPasswordBtn").addClass("d-none")

        $("#passwordDisplay").removeClass("d-none")
        $("#passwordInput1").addClass("d-none").val("")
        $("#passwordInput2").addClass("d-none").val("")
    })

    $("#editPhoneBtn").on("click", () => {
        $("#editPhoneBtn").addClass("d-none")
        $("#savePhoneBtn").removeClass("d-none")
        $("#cancelPhoneBtn").removeClass("d-none")

        $("#phoneDisplay").addClass("d-none")
        $("#phoneInput1").removeClass("d-none")
    })

    $("#savePhoneBtn").on("click", () => {
        if ($("#phoneInput1").val() != $("#phoneDisplay").text()) { alert("變更已儲存") }

        $("#phoneInput2").val($("#phoneInput1").val())
        $("#btn_phone").trigger("click")

        $("#editPhoneBtn").removeClass("d-none")
        $("#savePhoneBtn").addClass("d-none")
        $("#cancelPhoneBtn").addClass("d-none")

        $("#phoneDisplay").removeClass("d-none").text($("#phoneInput1").val())
        $("#phoneInput1").addClass("d-none").val($("#phoneDisplay").text())
    })

    $("#cancelPhoneBtn").on("click", () => {
        if ($("#phoneInput1").val() != $("#phoneDisplay").text()) { alert("變更未儲存，確定取消?") }

        $("#editPhoneBtn").removeClass("d-none")
        $("#savePhoneBtn").addClass("d-none")
        $("#cancelPhoneBtn").addClass("d-none")

        $("#phoneDisplay").removeClass("d-none")
        $("#phoneInput1").addClass("d-none").val($("#phoneDisplay").text())
    })

    $(".btn_cFav").on("click", function () {
        let collectionId = $(this).closest("tr").find(".td_F").text().trim();

        alert("變更已儲存")

        $("#collectionIdInput1").val(collectionId)
        $("#btn_cFavS").trigger("click")
    })

    $(".btn_dFav").on("click", function () {
        let collectionId = $(this).closest("tr").find(".td_F").text().trim();

        alert("變更已儲存")

        $("#collectionIdInput2").val(collectionId)
        $("#btn_dFavS").trigger("click")
    })

    $(".btn_uReservation").on("click", function () {
        let collectionId = $(this).closest("tr").find(".td_R").text().trim();

        alert("預約已取消")

        $("#id_collection2").val(collectionId)
        $("#btn_uReserveS").trigger("click")
    })

    $(".btn-open-comment").on("click", function () {
        let borrowId = $(this).closest("tr").find(".td_C").text().trim();
        let score = $(this).closest("tr").find(".td_score").text().trim();
        let comment = $(this).closest("tr").find(".td_feedback").text().trim();

        $("#borrowIdInput").val(borrowId)

        setSelectedStars(score)

        if (comment != "") { $("#commentText").val(comment); }
    })

    $(".btn_cCom").on("click", function () {
        let starCount = $(".bi-star-fill").length;

        alert("變更已儲存")

        $("#rate").val(starCount)
        $("#comment").val($("#commentText").val())
        $("#btn_comment").trigger("click")
    })

    $("#btn-tab5").on("click", () => { $("#btn_logout").trigger("click") })
})