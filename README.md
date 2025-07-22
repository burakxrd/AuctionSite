# *Auction Site 🚀*

Auction Site is a comprehensive web application developed with ASP.NET Core MVC, allowing users to participate in online auctions. Users can list their items for sale, bid on active items, and manage their auction activities. The platform provides a secure and dynamic environment for online bidding and selling.

*---*

Auction Site, ASP.NET Core MVC ile geliştirilmiş, kullanıcıların çevrimiçi açık artırmalara katılmasına olanak tanıyan kapsamlı bir web uygulamasıdır. Kullanıcılar, eşyalarını satışa listeleyebilir, aktif eşyalara teklif verebilir ve açık artırma faaliyetlerini yönetebilirler. Platform, çevrimiçi teklif verme ve satış için güvenli ve dinamik bir ortam sunar.

## **🌟 Key Features / 🌟 Temel Özellikler**

* **User Authentication & Authorization:** Secure user registration, login, and account management, including password reset and email change flows with email confirmation.
* **Auction Item Management:** Users can create, view, edit (under certain conditions), and delete their auction items.
* **Bidding System:** Users can place bids on active auction items, with validations for minimum bid increments and sufficient virtual balance.
* **Real-time Updates:** Countdown timers for auction items to show time remaining or time until start, providing dynamic updates without page refresh.
* **Auction Listing & Search:** Browse active, upcoming, and expired auctions. Advanced search functionality allows filtering and sorting by various criteria (price, time remaining, popularity).
* **Personal Auction & Bid Tracking:** Dedicated sections for users to view their listed auctions, bids they have placed, and auctions they have won.
* **Image Upload & Management:** Support for uploading images for auction items with validation for file type and size.
* **Localization:** The application supports multiple languages (English and Turkish) for a better user experience.
* **Virtual Balance System:** Users manage a virtual balance for placing bids and purchasing items.

*---*

* **Kullanıcı Kimlik Doğrulama ve Yetkilendirme:** Güvenli kullanıcı kaydı, giriş ve e-posta onayı ile şifre sıfırlama ve e-posta değiştirme akışları dahil hesap yönetimi.
* **Açık Artırma Öğesi Yönetimi:** Kullanıcılar, açık artırma öğelerini oluşturabilir, görüntüleyebilir, düzenleyebilir (belirli koşullar altında) ve silebilir.
* **Teklif Sistemi:** Kullanıcılar, minimum teklif artırımları ve yeterli sanal bakiye için doğrulama ile aktif açık artırma öğelerine teklif verebilirler.
* **Gerçek Zamanlı Güncellemeler:** Açık artırma öğeleri için kalan süreyi veya başlangıca kadar olan süreyi gösteren geri sayım sayaçları, sayfa yenilemeden dinamik güncellemeler sağlar.
* **Açık Artırma Listeleme ve Arama:** Aktif, yaklaşan ve süresi dolmuş açık artırmalara göz atın. Gelişmiş arama işlevselliği, çeşitli kriterlere göre (fiyat, kalan süre, popülerlik) filtreleme ve sıralama yapılmasına olanak tanır.
* **Kişisel Açık Artırma ve Teklif Takibi:** Kullanıcıların listeledikleri açık artırmaları, verdikleri teklifleri ve kazandıkları açık artırmaları görüntülemeleri için özel bölümler.
* **Görsel Yükleme ve Yönetimi:** Dosya türü ve boyutu doğrulaması ile açık artırma öğeleri için görsel yüklemeyi destekler.
* **Yerelleştirme:** Uygulama, daha iyi bir kullanıcı deneyimi için birden çok dili (İngilizce ve Türkçe) destekler.
* **Sanal Bakiye Sistemi:** Kullanıcılar, teklif vermek ve ürün satın almak için sanal bir bakiye yönetir.

## **🛠️ Technologies and Libraries Used / 🛠️ Kullanılan Teknolojiler ve Kütüphaneler**

* **Platform:** .NET 8 & ASP.NET Core MVC
* **Languages:** C#, HTML, CSS, JavaScript
* **Database:** Microsoft SQL Server
* **ORM:** Entity Framework Core
* **Authentication:** ASP.NET Core Identity with JWT Bearer authentication
* **UI Libraries:** Bootstrap, jQuery, jQuery UI
* **Image Processing:** SixLabors.ImageSharp
* **Email Sending:** `System.Net.Mail` (used in `EmailSender.cs`)
* **Localization:** `Microsoft.Extensions.Localization`, `Microsoft.AspNetCore.Mvc.Localization`

*---*

* **Platform:** .NET 8 ve ASP.NET Core MVC
* **Diller:** C#, HTML, CSS, JavaScript
* **Veritabanı:** Microsoft SQL Server
* **ORM:** Entity Framework Core
* **Kimlik Doğrulama:** JWT Bearer kimlik doğrulamalı ASP.NET Core Identity
* **UI Kütüphaneleri:** Bootstrap, jQuery, jQuery UI
* **Görsel İşleme:** SixLabors.ImageSharp
* **E-posta Gönderme:** `System.Net.Mail` (`EmailSender.cs` içinde kullanılmıştır)
* **Yerelleştirme:** `Microsoft.Extensions.Localization`, `Microsoft.AspNetCore.Mvc.Localization`

## **🚀 Setup and Getting Started / 🚀 Kurulum ve Başlarken**

Follow these steps to set up and run the project on your local machine.

*---*

Projeyi yerel makinenizde kurmak ve çalıştırmak için aşağıdaki adımları izleyin.

### **Prerequisites / Ön Koşullar**

* **Git:** Version control system. You can download it from [here](https://git-scm.com/downloads).
* **Visual Studio 2022 (or later):** With the ".NET web development" workload installed.
* **Microsoft SQL Server:** Any edition (e.g., free "Developer" or "Express" edition).
* **SQL Server Management Studio (SSMS):** For database management.
* **.NET 8 SDK:** Download and install from [Microsoft's official website](https://dotnet.microsoft.com/download/dotnet/8.0).

*---*

* **Git:** Versiyon kontrol sistemi. Buradan indirebilirsiniz: [Git İndirmeleri](https://git-scm.com/downloads).
* **Visual Studio 2022 (veya üzeri):** ".NET web geliştirme" iş yükü kurulu olmalıdır.
* **Microsoft SQL Server:** Herhangi bir sürüm (örn. ücretsiz "Developer" veya "Express" sürümü).
* **SQL Server Management Studio (SSMS):** Veritabanı yönetimi için.
* **.NET 8 SDK:** [Microsoft'un resmi web sitesinden](https://dotnet.microsoft.com/download/dotnet/8.0) indirin ve kurun.

### **Database Setup / Veritabanı Kurulumu**

1.  Open SQL Server Management Studio (SSMS) and connect to your SQL Server instance.
2.  Locate the `auctionScriptFile.sql` file in the project's root directory (`AuctionSite-c78fbdec2b225cc7b7e1aaa5a900529194b3fb65/auctionScriptFile.sql`) and open it within SSMS.
3.  Execute this SQL script (using the `Execute` button) to automatically create the database (`aspnet-AuctionSite-...`), all necessary tables, and initial schema.

*---*

1.  SQL Server Management Studio (SSMS) uygulamasını açın ve SQL Server örneğinize bağlanın.
2.  Projenin kök dizinindeki (`AuctionSite-c78fbdec2b225cc7b7e1aaa5a900529194b3fb65/auctionScriptFile.sql`) `auctionScriptFile.sql` dosyasını SSMS içinde açın.
3.  Bu SQL betiğini (`Execute` düğmesini kullanarak) çalıştırarak veritabanını (`aspnet-AuctionSite-...`), gerekli tüm tabloları ve başlangıç şemasını otomatik olarak oluşturun.

### **Application Configuration / Uygulama Yapılandırması**

Open your project in Visual Studio (`AuctionSite.sln`).

1.  **Database Connection String:**
    * The connection string is configured in `Program.cs` and typically sourced from `appsettings.json` or `appsettings.Development.json`.
    * Ensure the `DefaultConnection` string in your `appsettings.json` matches your SQL Server instance.
        ```json
        "ConnectionStrings": {
            "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=aspnet-AuctionSite-5a679155-148c-4063-8666-7c9f660dd1bc;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
        }
        ```
    * **Note:** If your SQL Server is running on the same machine, `Server=localhost` or `Server=.` (a single dot) might also work.

2.  **Email Settings (for Password Reset & Email Confirmation):**
    * SMTP settings are read from the configuration in `EmailSender.cs`. You need to configure these in your `appsettings.json` or `appsettings.Development.json`.
        ```json
        "SmtpSettings": {
            "Host": "smtp.example.com", // e.g., smtp.gmail.com
            "Port": "587",
            "Username": "your_email@example.com",
            "Password": "your_email_password_or_app_password",
            "SenderEmail": "your_email@example.com",
            "EnableSsl": "true"
        }
        ```
    * **Security Note:** For production applications, sensitive information like email passwords and full connection strings should not be hardcoded. Use environment variables or Azure Key Vault for secure storage.

3.  **JWT Settings (for API Authentication):**
    * JWT settings are read from the configuration in `Program.cs` and `AccountController.cs`. These should also be configured in `appsettings.json`.
        ```json
        "Jwt": {
            "Issuer": "AuctionSiteIssuer",
            "Audience": "AuctionSiteAudience",
            "Key": "YOUR_SUPER_SECRET_JWT_KEY_MIN_16_CHARS", // Replace with a strong, random key
            "ExpireDays": "7"
        }
        ```

*---*

Visual Studio'da projenizi açın (`AuctionSite.sln`).

1.  **Veritabanı Bağlantı Dizisi:**
    * Bağlantı dizisi `Program.cs` içinde yapılandırılmıştır ve genellikle `appsettings.json` veya `appsettings.Development.json`'dan alınır.
    * `appsettings.json` dosyanızdaki `DefaultConnection` dizisinin SQL Server örneğinizle eşleştiğinden emin olun.
        ```json
        "ConnectionStrings": {
            "DefaultConnection": "Server=SUNUCU_ADINIZ;Database=aspnet-AuctionSite-5a679155-148c-4063-8666-7c9f660dd1bc;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
        }
        ```
    * **Not:** SQL Server'ınız aynı makinede çalışıyorsa, `Server=localhost` veya `Server=.` (tek bir nokta) da çalışabilir.

2.  **E-posta Ayarları (Şifre Sıfırlama ve E-posta Onayı için):**
    * SMTP ayarları `EmailSender.cs` içindeki yapılandırmadan okunur. Bunları `appsettings.json` veya `appsettings.Development.json` dosyanızda yapılandırmanız gerekir.
        ```json
        "SmtpSettings": {
            "Host": "smtp.example.com", // örn: smtp.gmail.com
            "Port": "587",
            "Username": "e-postanız@example.com",
            "Password": "e-posta_şifreniz_veya_uygulama_şifreniz",
            "SenderEmail": "e-postanız@example.com",
            "EnableSsl": "true"
        }
        ```
    * **Güvenlik Notu:** Üretim uygulamaları için, e-posta şifreleri ve tam bağlantı dizeleri gibi hassas bilgiler doğrudan kaynak kodda tutulmamalıdır. Bunun yerine, ortam değişkenleri veya Azure Key Vault kullanılmalıdır.

3.  **JWT Ayarları (API Kimlik Doğrulaması için):**
    * JWT ayarları `Program.cs` ve `AccountController.cs` içindeki yapılandırmadan okunur. Bunlar da `appsettings.json` dosyasında yapılandırılmalıdır.
        ```json
        "Jwt": {
            "Issuer": "AuctionSiteIssuer",
            "Audience": "AuctionSiteAudience",
            "Key": "ÇOK_GİZLİ_JWT_ANAHTARINIZ_EN_AZ_16_KARAKTER", // Güçlü, rastgele bir anahtarla değiştirin
            "ExpireDays": "7"
        }
        ```

### **Running the Application / Uygulamayı Çalıştırma**

1.  Open the project in Visual Studio.
2.  Necessary NuGet packages should be automatically restored upon opening the solution.
3.  Press `F5` or click the "Start" button to launch the application.

*---*

1.  Projeyi Visual Studio'da açın.
2.  Çözümü açtığınızda gerekli NuGet paketleri otomatik olarak geri yüklenmelidir.
3.  Uygulamayı başlatmak için `F5` tuşuna basın veya "Başlat" düğmesine tıklayın.

## **📋 Application Usage Steps / 📋 Uygulama Kullanım Adımları**

Follow these steps to experience all features of the application:

*---*

Uygulamanın tüm özelliklerini deneyimlemek için aşağıdaki adımları izleyin:

1.  **Register an Account:** Click on "Register" to create a new user account.
2.  **Confirm Email:** After registration, check your email for a confirmation link to activate your account.
3.  **Log In:** Log in to the application with your new credentials.
4.  **Create a New Auction:** Navigate to "Create New Auction" to list an item for sale. Provide details such as product name, description, starting price, minimum bid increment, start time, end time, and an optional image.
5.  **Browse Auctions:** Explore active and upcoming auction items on the "Auction Items" or homepage.
6.  **Place Bids:** On an active auction's details page, enter a bid amount (ensuring it meets the minimum increment and your virtual balance) and place your bid.
7.  **Manage Account:** Access "Manage Account" to change your password, email, or phone number, and view your virtual balance.
8.  **View Your Auctions & Bids:** Check "Your Auctions" to see items you've listed and "Your Bids" to track bids you've placed.
9.  **View Winning Auctions:** See the auctions you have won on the "Winning Auctions" page.
10. **Search for Items:** Use the search bar in the navigation to find specific auction items and apply filters like price range or status.

*---*

1.  **Hesap Oluşturun:** Yeni bir kullanıcı hesabı oluşturmak için "Kaydol" düğmesine tıklayın.
2.  **E-postayı Onaylayın:** Kayıttan sonra, hesabınızı etkinleştirmek için e-postanızı kontrol ederek onay bağlantısına tıklayın.
3.  **Giriş Yapın:** Yeni kimlik bilgilerinizle uygulamaya giriş yapın.
4.  **Yeni Bir Açık Artırma Oluşturun:** Satılık bir öğe listelemek için "Yeni Açık Artırma Oluştur" bölümüne gidin. Ürün adı, açıklama, başlangıç fiyatı, minimum teklif artırımı, başlangıç zamanı, bitiş zamanı ve isteğe bağlı bir görsel gibi ayrıntıları sağlayın.
5.  **Açık Artırmalara Göz Atın:** "Açık Artırma Ürünleri" sayfasında veya ana sayfada aktif ve yaklaşan açık artırma öğelerine göz atın.
6.  **Teklif Verin:** Aktif bir açık artırmanın detay sayfasında, bir teklif miktarı girin (minimum artışı ve sanal bakiyenizi karşıladığından emin olun) ve teklifinizi verin.
7.  **Hesabı Yönetin:** Şifrenizi, e-postanızı veya telefon numaranızı değiştirmek ve sanal bakiyenizi görüntülemek için "Hesabı Yönet" bölümüne erişin.
8.  **Açık Artırmalarınızı ve Tekliflerinizi Görüntüleyin:** Listelediğiniz öğeleri görmek için "Açık Artırmalarım" bölümünü ve verdiğiniz teklifleri takip etmek için "Tekliflerim" bölümünü kontrol edin.
9.  **Kazanılan Açık Artırmaları Görüntüleyin:** "Kazanılan Açık Artırmalar" sayfasında kazandığınız açık artırmaları görün.
10. **Öğeleri Arayın:** Belirli açık artırma öğelerini bulmak ve fiyat aralığı veya durum gibi filtreleri uygulamak için gezinme çubuğundaki arama çubuğunu kullanın.

## **🤝 Contributing / 🤝 Katkıda Bulunma**

This project is developed for personal demonstration and learning purposes. Contributions are not currently accepted from external sources. However, you are welcome to share your ideas and suggestions via the "Issues" section.

*---*

Bu proje kişisel gösterim ve öğrenme amaçlı geliştirilmiştir. Dış kaynaklardan gelen katkılar şu anda kabul edilmemektedir. Ancak, fikir ve önerilerinizi "Sorunlar" bölümü aracılığıyla paylaşabilirsiniz.

## **📄 License / 📄 Lisans**

This project is licensed under the MIT License. See the LICENSE file for more information.

*---*

Bu proje MIT Lisansı altında lisanslanmıştır. Daha fazla bilgi için LİSANS dosyasına bakın.
