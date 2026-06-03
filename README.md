# 🎓 EduChatbot — AI-Powered Learning Platform

<div align="center">

![.NET](https://img.shields.io/badge/.NET_9-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![Google Gemini](https://img.shields.io/badge/Google_Gemini-4285F4?style=for-the-badge&logo=google&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap_5-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)

**Hệ thống quản lý môn học & chatbot thông minh sử dụng công nghệ RAG (Retrieval-Augmented Generation)**

</div>

---

## 📋 Mục lục

- [Giới thiệu](#-giới-thiệu)
- [Tính năng nổi bật](#-tính-năng-nổi-bật)
- [Kiến trúc hệ thống](#-kiến-trúc-hệ-thống)
- [Công nghệ sử dụng](#-công-nghệ-sử-dụng)
- [Cài đặt & Chạy dự án](#-cài-đặt--chạy-dự-án)
- [Cấu trúc dự án](#-cấu-trúc-dự-án)
- [Phân quyền người dùng](#-phân-quyền-người-dùng)
- [Hướng dẫn sử dụng](#-hướng-dẫn-sử-dụng)
- [Tài khoản mặc định](#-tài-khoản-mặc-định)

---

## 🌟 Giới thiệu

**EduChatbot** là một ứng dụng web giáo dục được xây dựng trên nền tảng **ASP.NET Core 9 MVC**, tích hợp trí tuệ nhân tạo thông qua **Google Gemini API** với công nghệ **RAG (Retrieval-Augmented Generation)**.

Hệ thống cho phép giảng viên tải lên tài liệu học tập (PDF, TXT, DOCX), sau đó AI sẽ tự động đọc, phân tích và lập chỉ mục nội dung. Sinh viên có thể đặt câu hỏi trực tiếp với AI và nhận được câu trả lời chính xác dựa trên tài liệu của môn học.

### Luồng hoạt động RAG

```
Tài liệu (PDF/TXT)
        │
        ▼
 Trích xuất văn bản
        │
        ▼
  Phân đoạn (chunk)
  ~300 từ / đoạn
        │
        ▼
  Google Gemini API
  Tạo vector embedding
  (768 chiều)
        │
        ▼
 Lưu vào PostgreSQL
   + pgvector
        │
        ▼
    [Khi chat]
        │
        ▼
  Câu hỏi → Embedding
        │
        ▼
  Cosine Similarity Search
  → Top 3 đoạn liên quan
        │
        ▼
  Gemini GenerateContent
  (prompt + context)
        │
        ▼
      Trả lời
```

---

## ✨ Tính năng nổi bật

### 🤖 AI & RAG
- Tự động đọc và học nội dung tài liệu PDF/TXT
- Tìm kiếm ngữ nghĩa (semantic search) bằng vector embedding
- Chatbot trả lời dựa trên nội dung tài liệu môn học
- Sử dụng mô hình `gemini-2.5-flash` và `gemini-embedding-001`

### 📚 Quản lý học tập
- Quản lý môn học (tạo, sửa, xóa)
- Upload và quản lý tài liệu môn học
- Xem tài liệu PDF trực tiếp trên trình duyệt
- Tải tài liệu về máy

### 👥 Quản lý người dùng
- Phân quyền 3 cấp: Admin, Giảng viên, Sinh viên
- Admin quản lý toàn bộ tài khoản (tạo, xóa)
- Admin gán Giảng viên/Sinh viên vào môn học
- Giảng viên chỉ thêm được Sinh viên vào môn học của mình

### 🎨 Giao diện
- Sidebar navigation hiện đại
- Card layout cho danh sách môn học
- SweetAlert2 thay thế confirm dialog mặc định
- Responsive design (mobile-friendly)

---

## 🏗️ Kiến trúc hệ thống

Dự án tuân theo kiến trúc **3-Layer** nghiêm ngặt:

```
┌─────────────────────────────────────────────┐
│           Tầng Trình bày (MVC)              │
│  Controllers / Views / DTOs (BLL)           │
│  Không import trực tiếp DAL entities        │
├─────────────────────────────────────────────┤
│         Tầng Nghiệp vụ (BLL)               │
│  Services / DTOs / Helpers                  │
│  Chứa toàn bộ logic nghiệp vụ              │
├─────────────────────────────────────────────┤
│        Tầng Truy cập dữ liệu (DAL)         │
│  Repositories / Entities / DbContext        │
│  Chỉ tương tác với Database                │
└─────────────────────────────────────────────┘
                      │
                      ▼
              PostgreSQL + pgvector
```

**Nguyên tắc kiến trúc:**
- MVC chỉ giao tiếp với BLL thông qua **DTOs**, không bao giờ import DAL entities
- BLL chứa toàn bộ business logic, giao tiếp với DAL qua **Repository interfaces**
- DAL chỉ chứa logic truy cập database, không chứa business logic

---

## 🛠️ Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| **Framework** | ASP.NET Core 9 MVC |
| **ORM** | Entity Framework Core 9 |
| **Database** | PostgreSQL 18 |
| **Vector Store** | pgvector 0.3 |
| **AI Model** | Google Gemini 2.5 Flash |
| **Embedding** | Google Gemini Embedding-001 (768 dim) |
| **PDF Parser** | PdfPig 0.1.14 |
| **Authentication** | ASP.NET Cookie Authentication |
| **Frontend** | Bootstrap 5, Bootstrap Icons |
| **Alert Dialog** | SweetAlert2 |
| **Font** | Inter (Google Fonts) |

---

## 🚀 Cài đặt & Chạy dự án
### Yêu cầu

- [.NET SDK 9.0+](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/) với extension **pgvector**
- Google Gemini API Key 


---

## 📁 Cấu trúc dự án

```
PRN222-Assiment1/
│
├── RagChatbot.DAL/                    # Data Access Layer
│   ├── Data/
│   │   └── ApplicationDbContext.cs    # EF Core DbContext
│   ├── Entities/                      # Database entities
│   │   ├── User.cs
│   │   ├── Subject.cs
│   │   ├── Document.cs
│   │   ├── DocumentChunk.cs           # Vector embeddings
│   │   ├── DocumentStatus.cs
│   │   └── UserSubject.cs             # Bảng quan hệ User-Subject
│   ├── Repositories/
│   │   ├── Interfaces/                # Repository interfaces
│   │   └── Implements/                # Repository implementations
│   └── Migrations/                    # EF Core migrations
│
├── RagChatbot.BLL/                    # Business Logic Layer
│   ├── DTOs/                          # Data Transfer Objects
│   │   ├── UserDto.cs
│   │   ├── UserManageDto.cs
│   │   ├── SubjectDto.cs
│   │   └── DocumentDto.cs
│   ├── Services/
│   │   ├── Interfaces/                # Service interfaces
│   │   └── Implements/
│   │       ├── UserService.cs
│   │       ├── SubjectService.cs
│   │       ├── DocumentService.cs
│   │       ├── DocumentProcessingService.cs  # RAG pipeline
│   │       ├── GeminiService.cs              # AI integration
│   │       ├── ChatbotService.cs             # Vector search + chat
│   │       └── UserSubjectService.cs
│   ├── Helpers/
│   │   └── TextChunker.cs             # Text chunking logic
│   └── Extensions/
│       └── ServiceCollectionExtensions.cs   # DI registration
│
└── RagChatbot.MVC/                    # Presentation Layer
    ├── Controllers/
    │   ├── AccountController.cs
    │   ├── HomeController.cs
    │   ├── SubjectController.cs
    │   ├── DocumentController.cs
    │   ├── ChatController.cs
    │   ├── UserController.cs
    │   └── MemberController.cs
    ├── Views/
    │   ├── Account/Login.cshtml
    │   ├── Home/Index.cshtml
    │   ├── Subject/              (Index, Create, Edit)
    │   ├── Document/             (Index, Create, ViewDoc)
    │   ├── Chat/Index.cshtml
    │   ├── User/                 (Index, Create)
    │   ├── Member/               (Index, Add)
    │   └── Shared/_Layout.cshtml
    ├── wwwroot/
    │   ├── css/site.css          # Custom design system
    │   └── uploads/              # Uploaded documents
    └── Program.cs
```

---

## 📖 Hướng dẫn sử dụng

### Quy trình sử dụng cơ bản

```
1. Admin đăng nhập
   └─→ Tạo môn học
   └─→ Gán Giảng viên vào môn học
   └─→ Gán Sinh viên vào môn học

2. Giảng viên đăng nhập
   └─→ Xem môn học được gán
   └─→ Upload tài liệu PDF/TXT
       └─→ AI tự động xử lý & lập chỉ mục
   └─→ Thêm Sinh viên vào môn học

3. Sinh viên đăng nhập
   └─→ Xem môn học được gán
   └─→ Xem tài liệu
   └─→ Chat với AI để hỏi về nội dung tài liệu
```

### Upload tài liệu & AI xử lý

1. Vào **Môn học** → **Xem Tài liệu** → **Upload Tài Liệu Mới**
2. Kéo thả hoặc chọn file (PDF hoặc TXT)
3. Hệ thống tự động:
   - Lưu file vào server
   - Trích xuất văn bản
   - Phân đoạn thành các chunk ~300 từ
   - Gọi Gemini API tạo vector embedding cho từng chunk
   - Lưu vào PostgreSQL với pgvector
4. Trạng thái chuyển từ **Đang xử lý** → **AI đã học xong**

### Chat với AI

1. Vào **Môn học** → **Chat AI**
2. Nhập câu hỏi liên quan đến nội dung tài liệu
3. AI sẽ:
   - Chuyển câu hỏi thành vector embedding
   - Tìm kiếm cosine similarity trong database
   - Lấy top 3 đoạn văn bản liên quan nhất
   - Gửi context + câu hỏi cho Gemini để tạo câu trả lời

---

## 📝 Lưu ý

- File `appsettings.json` chứa thông tin nhạy cảm — **không commit lên GitHub** (đã có trong `.gitignore`)
- Thư mục `wwwroot/uploads/` chứa file người dùng tải lên — **không commit**
- Mật khẩu trong seed data là plain-text (`123`) — chỉ dùng cho môi trường development

---

<div align="center">

**PRN222 Assignment — FPT University**

Made with ❤️ using ASP.NET Core 9 & Google Gemini AI

</div>
