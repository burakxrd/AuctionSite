// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Deprecated uyarılarını gidermek için $(document).ready() yerine kısa yazım kullanıldı.
$(function () {
    // Autocomplete işlevselliği için
    // Arama kutusunun ID'si "searchInput" olarak ayarlandı
    $("#searchInput").autocomplete({
        source: function (request, response) {
            // Sunucuya AJAX isteği gönder
            $.ajax({
                url: "/AuctionItems/Suggest", // AuctionItemsController'daki Suggest aksiyonuna yönlendir
                dataType: "json",
                data: {
                    term: request.term // Kullanıcının yazdığı terimi gönder
                },
                success: function (data) {
                    // Sunucudan gelen veriyi autocomplete listesi olarak göster
                    response(data);
                },
                error: function (xhr, status, error) {
                    console.error("Autocomplete isteği başarısız oldu:", status, error);
                }
            });
        },
        minLength: 2, // En az 2 karakter girildiğinde önerileri göster
        select: function (event, ui) {
            // Kullanıcı bir öneri seçtiğinde arama kutusunu seçilen değerle doldur
            $("#searchInput").val(ui.item.value);
            // Formu otomatik göndermek için submit() metodunun parametresiz çağrımı kullanılır.
            // Bu uyarıyı alıyorsanız, jQuery versiyonunuzun daha yeni bir versiyonu olabilir
            // veya jQuery UI'ın jQuery ile uyumluluğu konusunda bir uyarı olabilir.
            // Fonksiyonel olarak doğru çalışmaya devam edecektir.
            $(this).closest('form')[0].submit(); // jQuery nesnesinden DOM elementine erişip submit çağrısı
        }
    });
});
