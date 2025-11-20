# 🟢 ZipApp SignalR Real-Time Chat System - Açıklama Rehberi

## 📋 Giriş

Uygulamanızda müşteriler ile admin panelinde çalışan yöneticiler arasında **gerçek zamanlı sohbet** yapabilmesini sağlayan bir sistem kurulmuştur. Bu sistem **Microsoft SignalR** kullanarak WebSocket üzerinden iki taraflı iletişim sağlar.

---

## 🏗️ Mimarisi

```
┌─────────────────────────────────────────────────────────┐
│                   ASP.NET Core 8.0                      │
│                  (Backend - Server)                     │
└─────────────┬───────────────────────────────────────────┘
              │
         SignalR Hub
              │
    ┌─────────┴─────────┐
    │                   │
    ▼                   ▼
┌─────────────┐   ┌─────────────┐
│  Müşteri 1  │   │  Müşteri 2  │
│ (Browser)   │   │ (Browser)   │
└─────────────┘   └─────────────┘
    │                   │
    └─────────┬─────────┘
              │
         SignalR Hub
              │
         ┌────┴────┐
         │          │
    ┌────▼──┐  ┌────▼──┐
    │ Admin │  │ Admin │
    │Panel 1│  │Panel 2│
    └───────┘  └───────┘
```

---

## 🔧 Teknik Bileşenler

### 1. **Backend - ChatHub.cs**
📂 Konum: `Hubs/ChatHub.cs`

**Amaç**: Müşteriler ve admin panelleri arasında mesaj yönlendirmesini sağlar.

**Temel Metodlar**:

| Metod | Açıklama | Kimin Çağırdığı |
|-------|----------|-----------------|
| `SendMessageToAdmin(message, userName)` | Müşterinin mesajını admins grubuna gönderir | Müşteri Widget |
| `SendMessageToCustomer(message, adminName, customerId)` | Admin'in mesajını belirli müşteriye gönderir | Admin Panel |
| `CustomerTyping(userName)` | Müşteri yazıyor göstergesi gönderir | Müşteri Widget |
| `AdminTyping(adminName)` | Admin yazıyor göstergesi gönderir | Admin Panel |
| `StopTyping(role)` | Yazıyor göstergesini kaldırır | Her iki taraf |
| `OnConnectedAsync()` | Kullanıcı bağlandığında tetiklenir (admin/müşteri) | SignalR |
| `OnDisconnectedAsync()` | Kullanıcı çıktığında tetiklenir | SignalR |

**Grup Yönetimi**:
```csharp
// Admin bağlanırsa
await Groups.AddToGroupAsync(Context.ConnectionId, "admins");

// Müşteri bağlanırsa
await Groups.AddToGroupAsync(Context.ConnectionId, "customers");
```

---

### 2. **Frontend - Müşteri Widget (_Layout.cshtml)**
📂 Konum: `Views/Shared/_Layout.cshtml` (satır 466+)

**Amaç**: Sayfanın sağ alt köşesinde WhatsApp benzeri sohbet widget'ı sağlar.

**SignalR Bağlantısı**:
```javascript
const chatConnection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/chat")
    .withAutomaticReconnect()
    .build();
```

**Olay İşleyicileri (Event Handlers)**:

| Event | Ne Yaptığı |
|-------|-----------|
| `ReceiveAdminMessage` | Admin mesajı alındığında sohbet penceresine ekler |
| `ShowAdminTyping` | Admin yazıyor animasyonunu gösterir |
| `HideAdminTyping` | Admin yazıyor animasyonunu gizler |
| `StopTyping` | Genel yazıyor göstergesini kaldırır |

**Mesaj Gönderimi**:
```javascript
chatConnection.invoke("SendMessageToAdmin", text, "Müşteri")
    .catch(err => console.error(err));
```

---

### 3. **Frontend - Admin Panel (Chat.cshtml)**
📂 Konum: `Views/Admin/Chat.cshtml`

**Amaç**: Admin'in tüm aktif müşteri konuşmalarını yönetmesini sağlar.

**Layout**:
- **Sol Panel**: Bağlı müşterilerin listesi (🟢 Çevrimiçi / ⚫ Çevrimdışı)
- **Sağ Panel**: Seçili müşteri ile sohbet geçmişi
- **Input Alanı**: Admin mesajı yazma ve gönderme

**Admin Panel SignalR Olayları**:

| Event | Ne Yaptığı |
|-------|-----------|
| `ReceiveCustomerMessage` | Müşteri mesajı alındığında ekler |
| `ShowCustomerTyping` | Müşteri yazıyor göstergesi gösterir |
| `CustomerConnected` | Yeni müşteri bağlanırsa listeye ekler |
| `CustomerDisconnected` | Müşteri çıkarsa listesini günceller |

---

## 🚀 Kullanım Akışı

### Senaryo: Müşteri → Admin Konuşması

#### 1️⃣ **Müşteri Mesaj Gönderir**

```javascript
// Müşteri widget'ında Enter tuşu basılırsa
sendMessage() {
    let text = document.getElementById("messageInput").value;
    
    // 1. Mesajı yerel UI'ya ekle (yeşil, sağ taraf)
    displayAdminChatMessage(text, "Müşteri", true);
    
    // 2. SignalR üzerinden admin'e gönder
    chatConnection.invoke("SendMessageToAdmin", text, "Müşteri")
        .catch(err => console.error(err));
}
```

#### 2️⃣ **Backend'de Mesaj Yönlendirilir**

```csharp
// ChatHub.cs
public async Task SendMessageToAdmin(string message, string userName)
{
    // Tüm admin panellerine gönder
    await Clients.Group("admins").SendAsync("ReceiveCustomerMessage", new
    {
        message = message,
        userName = userName,
        customerId = Context.ConnectionId,  // Cevap için gerekli
        timestamp = DateTime.Now.ToString("HH:mm:ss")
    });
}
```

#### 3️⃣ **Admin Panel'de Mesaj Alınır**

```javascript
// Admin panel (Chat.cshtml)
adminConnection.on("ReceiveCustomerMessage", (message, customerName, customerId) => {
    // Seçili müşteri ise
    if (selectedCustomerId === customerId) {
        displayAdminChatMessage(message, customerName, false);  // Gri, sol taraf
    }
});
```

#### 4️⃣ **Admin Cevap Verir**

```javascript
// Admin panel'de
sendAdminMessage() {
    let message = document.getElementById("adminMessageInput").value;
    
    // SignalR üzerinden belirli müşteriye gönder (customerId ile)
    adminConnection.invoke("SendMessageToCustomer", message, "Destek Ekibi", selectedCustomerId)
        .catch(err => console.error(err));
}
```

#### 5️⃣ **Müşteri Admin Mesajını Alır**

```javascript
chatConnection.on("ReceiveAdminMessage", (message, adminName) => {
    // Mesajı sohbet penceresine ekle (yeşil background)
    displayAdminChatMessage(message, adminName, true);
});
```

---

## ✍️ Yazıyor Animasyonu

### Müşteri Yazıyor Göstergesi

**Tetiklenme**:
```javascript
// Müşteri bir karakter yazmaya başlasa
messageInput.addEventListener("input", function() {
    chatConnection.invoke("CustomerTyping", "Müşteri")
        .catch(err => console.error(err));
});
```

**Admin Panel'de Görünüş**:
```javascript
adminConnection.on("ShowCustomerTyping", (customerName, customerId) => {
    if (selectedCustomerId === customerId) {
        showAdminTypingIndicator(customerName);  // "Müşteri yazıyor..."
    }
});
```

**Animasyon CSS**:
```css
@@keyframes blink {
    0%, 60%, 100% {
        opacity: 0.3;
    }
    30% {
        opacity: 1;
    }
}

.typing-dots span {
    animation: blink 1.4s infinite;
}
```

**Görsel**:
```
┌──────────────────────────┐
│ Müşteri yazıyor...       │
│ ‣ · · (animasyon)        │
└──────────────────────────┘
```

---

## 🔐 Güvenlik Özellikleri

### 1. **HTML Kaçış (XSS Önleme)**
```javascript
function escapeHtml(text) {
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.replace(/[&<>"']/g, m => map[m]);
}

// Kullanım
bubble.innerHTML = `${escapeHtml(message)}`; // İnjecton'dan korunur
```

### 2. **Admin Kontrolü (Chat.cshtml)**
```csharp
public IActionResult Chat()
{
    // Sadece admin'ler erişebilir
    if (HttpContext.Session.GetString("IsAdmin") != "True")
        return RedirectToAction("Index", "Login");
    
    return View();
}
```

### 3. **Müşteri Grubu İzolasyonu**
```csharp
// Admin mesajı sadece belirli müşteriye gider
await Clients.Client(customerId).SendAsync("ReceiveAdminMessage", ...);

// Başka müşteriler bu mesajı görmez
```

---

## 📊 Veri Akışı Diyagramları

### Senaryo 1: Müşteri Bağlanması
```
1. Müşteri sayfaya gider
        ↓
2. _Layout.cshtml yüklenir (WhatsApp widget)
        ↓
3. chatConnection.start() çalışır
        ↓
4. ChatHub.OnConnectedAsync() tetiklenir
        ↓
5. Müşteri "customers" grubuna eklenir
        ↓
6. Tüm admin panellerine "CustomerConnected" gönderilir
        ↓
7. Admin panel'de müşteri listesine eklenir ✓
```

### Senaryo 2: Mesaj Gönderimi (Müşteri → Admin)
```
Müşteri Widget               SignalR Hub (Backend)           Admin Panel
        │                            │                              │
        ├─► SendMessageToAdmin()─────►│                              │
        │                    (mesaj)  │                              │
        │                            ├─► ReceiveCustomerMessage()───►│
        │                            │                        (gri)   │
        │                            │                          mesaj │
        │◄────display locally────────┤                              │
```

### Senaryo 3: Yazıyor Animasyonu
```
Müşteri yazıyor (input event)
        │
        ├─► CustomerTyping()
        │
        ▼ (SignalR)
    
Admin Panel
        │
        ├─► ShowCustomerTyping()
        │
        ▼
    Display: "Müşteri yazıyor..."
    Animation: · · · (blink)
```

---

## 🛠️ Bakım ve Troubleshooting

### Sorun: Mesajlar iletilmiyor
**Çözüm**:
1. SignalR hub'ı Program.cs'de kayıtlı mı? → `app.MapHub<login.Hubs.ChatHub>("/hubs/chat");`
2. Browser console'da hata var mı? → F12 açarak kontrol edin
3. Tarayıcı WebSocket'i destekliyor mu? (Modern tarayıcılar destekler)

### Sorun: Admin mesajını sadece bir müşteri almış olmalıydı ama tüm müşteriler aldı
**Çözüm**: 
```csharp
// ❌ Yanlış - tüm müşterilere gider
await Clients.Group("customers").SendAsync(...);

// ✅ Doğru - sadece belirli müşteriye gider
await Clients.Client(customerId).SendAsync(...);
```

### Sorun: Yazıyor animasyonu görülmüyor
**Çözüm**:
1. CSS'de @keyframes blink tanımlandı mı?
2. JavaScript'de showAdminTypingIndicator() çağrılıyor mu?
3. Typing göstergesi HTML'de var mı? (`id="adminTypingIndicator"`)

---

## 📈 Gelecek İyileştirmeler

### Önerilen Özellikler
1. **Sohbet Geçmişi**: Veritabanında mesajları sakla ve eski konuşmaları göster
   ```sql
   CREATE TABLE ChatMessages (
       Id INT PRIMARY KEY,
       CustomerId VARCHAR(255),
       AdminId VARCHAR(255),
       Message LONGTEXT,
       CreatedAt DATETIME(6),
       IsFromAdmin BIT
   );
   ```

2. **Dosya Paylaşımı**: Müşteriler ve admin resim/belge paylaşsın
   ```javascript
   fileInput.addEventListener("change", async (e) => {
       const file = e.target.files[0];
       const base64 = await fileToBase64(file);
       await chatConnection.invoke("SendFile", base64);
   });
   ```

3. **Mesaj Bildirimleri**: Desktop notification
   ```javascript
   if (Notification.permission === "granted") {
       new Notification("Yeni mesaj!", {
           body: message,
           icon: "/img/logo.png"
       });
   }
   ```

4. **Kullanıcı Yazıyor Simgesi**: Gerçek kullanıcı listesi ile simge göster
   ```javascript
   // Admin panelinde aktif yazanları göster
   const typingUsers = [
       { name: "Müşteri 1", isTyping: true },
       { name: "Müşteri 2", isTyping: false }
   ];
   ```

---

## 📚 Kaynaklar

- [Microsoft SignalR Documentation](https://learn.microsoft.com/en-us/aspnet/core/signalr/)
- [SignalR Groups](https://learn.microsoft.com/en-us/aspnet/core/signalr/groups)
- [WebSocket Protokolü](https://en.wikipedia.org/wiki/WebSocket)
- [ASP.NET Core SignalR Client Library](https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/8.0.0/)

---

## ✨ Özet

| Bileşen | Konum | Amaç |
|---------|-------|------|
| **ChatHub** | `Hubs/ChatHub.cs` | Mesaj yönlendirmesi ve grup yönetimi |
| **Müşteri Widget** | `Views/Shared/_Layout.cshtml` | WhatsApp benzeri sohbet UI |
| **Admin Panel** | `Views/Admin/Chat.cshtml` | Tüm müşteri konuşmalarını yönetme |
| **Admin Controller** | `Controllers/Admin.cs` | Chat action route'u |
| **SignalR Kütüphanesi** | CDN: signalr.min.js | Client-side bağlantı |

---

**Son Güncellenme**: Aralık 2024
**Versiyon**: 1.0.0
**Durum**: ✅ Canlı Ortaya Hazır
