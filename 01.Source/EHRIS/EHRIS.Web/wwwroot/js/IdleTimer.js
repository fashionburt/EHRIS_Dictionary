(function () {

    let idleSeconds = parseInt(window.IdleConfig.idleSeconds);
    let warningSeconds = parseInt(window.IdleConfig.warningSeconds);
    let logoutUrl = window.IdleConfig.logoutUrl;

    let remaining = idleSeconds;
    let warningShown = false;
    let swalTimerInterval = null;
    function fmt(sec) {
        sec = Math.max(0, parseInt(sec));
        let m = String(Math.floor(sec / 60)).padStart(2, '0');
        let s = String(sec % 60).padStart(2, '0');
        return `${m}:${s}`;
    }

    function resetTimer() {
     
        remaining = idleSeconds;
        document.getElementById("idle-timer-count").textContent = fmt(remaining);
    }

    
    // 只有真正「有操作」時才 reset
    ["click", "keydown", "mousedown", "scroll", "touchstart"].forEach(evt => {
        document.addEventListener(evt, resetTimer);
    });
    // ===== 啟動計時器 =====
    function startIdleTimer() {
        resetTimer();

        intervalId = setInterval(() => {
            remaining--;
            document.getElementById("idle-timer-count").textContent = fmt(remaining);

            // ======== 進入警告 ========
            if (remaining === warningSeconds) {
                showIdleWarning();
            }

            // ======== 時間到 → 自動登出 ========
            if (remaining <= 0) {
                clearInterval(intervalId);
                window.location.href = window.IdleConfig.logoutUrl + "?timeout=1";
            }
        }, 1000);
    }
    
    // ===== SweetAlert2 閒置警告 =====
    function showIdleWarning() {
        let swalSeconds = warningSeconds;

        Swal.fire({
            title: "閒置警告",
            html: `您已閒置太久，<b>${swalSeconds}</b> 秒後將自動登出。<br><br>
               <span class="text-danger">是否要保持登入？</span>`,
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "保持登入",
            cancelButtonText: "重新登入",
            allowOutsideClick: false,
            allowEscapeKey: false,
            didOpen: () => {
                swalTimerInterval = setInterval(() => {
                    swalSeconds--;
                    Swal.getHtmlContainer().querySelector("b").textContent = swalSeconds;

                    if (swalSeconds <= 0) {
                        clearInterval(swalTimerInterval);
                        Swal.close(); 
                    }
                }, 1000);
            }
        }).then(result => {
            clearInterval(swalTimerInterval);

            if (result.isConfirmed) {
                // ======= ajax 延長 session =======
                $.post(window.IdleConfig.keepAlive, function (res) {
                    if (res.success) {
                        resetTimer();
                    }
                });

            } else {
                // ======= 使用者取消 / 倒數歸零 =======
                window.location.href = window.IdleConfig.logoutUrl;
            }
        });
    }
    startIdleTimer();
})();
