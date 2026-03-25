var checkExist = setInterval(function () {
    var authButton = document.querySelector('.btn.authorize');

    if (authButton) {
        clearInterval(checkExist);

        // DİKKAT: Artık butonu kopyalayıp bozmuyoruz. 
        // true parametresi (Capture Phase) ile tıklamayı Swagger'ın React eventlerinden ÖNCE biz yakalıyoruz.
        authButton.addEventListener('click', function (e) {

            // Swagger'ın kendi belleğinden "Bearer" token'ı var mı diye kontrol ediyoruz
            var isAuthorized = window.ui.authSelectors.authorized().get("Bearer");

            if (!isAuthorized) {
                // DURUM 1: KULLANICI GİRİŞ YAPMAMIŞ
                // Swagger'ın varsayılan ekranını engelle ve kendi özel sayfamızı aç
                e.stopPropagation();
                e.preventDefault();

                var loginWindow = window.open('/login.html', 'FinanceLogin', 'width=400,height=500,top=200,left=500');

                if (!loginWindow) {
                    alert("Tarayıcınız pop-up'ı engelledi. Lütfen izin verin.");
                    return;
                }

                window.addEventListener('message', function (event) {
                    if (event.data && event.data.token) {
                        var tokenValue = "Bearer " + event.data.token;

                        // Token'ı Swagger'a enjekte et
                        window.ui.authActions.authorize({
                            Bearer: { name: "Bearer", schema: { type: "apiKey", in: "header", name: "Authorization", description: "" }, value: tokenValue }
                        });

                        loginWindow.close();
                    }
                }, { once: true });
            }
            // DURUM 2: KULLANICI GİRİŞ YAPMIŞ
            // Hiçbir şey yapmıyoruz. e.stopPropagation() çalışmadığı için 
            // tıklama Swagger'a ulaşacak ve o şık "Logout" penceresi sorunsuzca açılacak!

        }, true); // <- İşte sihir burada: Capture Phase
    }
}, 500);