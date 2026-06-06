using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using RagChatbot.BLL.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace RagChatbot.MVC.Controllers
{
    /// <summary>
    /// Controller tạm thời — dùng một lần để seed tài liệu IT vào DB.
    /// Chỉ Admin mới truy cập được.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SeederController : Controller
    {
        private readonly ISubjectService _subjectService;
        private readonly IDocumentService _documentService;
        private readonly IDocumentProcessingService _processingService;
        private readonly IWebHostEnvironment _env;

        public SeederController(
            ISubjectService subjectService,
            IDocumentService documentService,
            IDocumentProcessingService processingService,
            IWebHostEnvironment env)
        {
            _subjectService  = subjectService;
            _documentService = documentService;
            _processingService = processingService;
            _env = env;
        }

        // GET /Seeder — trang xác nhận
        [HttpGet]
        public IActionResult Index()
        {
            var subjects = _subjectService.GetAllSubjects().ToList();
            ViewBag.Subjects = subjects;
            ViewBag.DocCount = subjects.Count * 3;
            return View();
        }

        // POST /Seeder/Run — thực thi seed
        [HttpPost]
        public async Task<IActionResult> Run()
        {
            var subjects = _subjectService.GetAllSubjects().ToList();
            if (!subjects.Any())
            {
                TempData["ErrorMessage"] = "Không có môn học nào trong hệ thống. Hãy tạo môn học trước.";
                return RedirectToAction("Index");
            }

            var templates = GetDocumentTemplates();
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var results = new List<string>();
            int templateIndex = 0;

            foreach (var subject in subjects)
            {
                int docsForThisSubject = 3;

                for (int i = 0; i < docsForThisSubject; i++)
                {
                    var (fileName, content) = templates[templateIndex % templates.Count];
                    templateIndex++;

                    // Tên file duy nhất để tránh trùng lặp
                    string uniqueFileName = $"{Path.GetFileNameWithoutExtension(fileName)}-{Guid.NewGuid().ToString("N")[..6]}.txt";

                    try
                    {
                        // 1. Upload tài liệu qua service
                        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                        var uploadResult = await _documentService.UploadDocumentAsync(
                            subject.Id, uniqueFileName, stream, uploadsFolder);

                        if (uploadResult == DocumentUploadResult.Duplicate)
                        {
                            results.Add($"⚠️ [{subject.Code}] {uniqueFileName} — đã tồn tại, bỏ qua");
                            continue;
                        }
                        if (uploadResult == DocumentUploadResult.Error)
                        {
                            results.Add($"❌ [{subject.Code}] {uniqueFileName} — upload thất bại");
                            continue;
                        }

                        // 2. Tìm document vừa tạo
                        var doc = _documentService.GetDocumentsBySubject(subject.Id)
                                                  .OrderByDescending(d => d.UploadedAt)
                                                  .FirstOrDefault(d => d.FileName == uniqueFileName);

                        if (doc == null)
                        {
                            results.Add($"❌ [{subject.Code}] {uniqueFileName} — không tìm thấy sau upload");
                            continue;
                        }

                        // 3. Xử lý RAG (semantic chunk + embedding)
                        bool processed = await _processingService.ProcessDocumentAsync(doc.Id, _env.WebRootPath);
                        results.Add(processed
                            ? $"✅ [{subject.Code}] {uniqueFileName}"
                            : $"⚠️ [{subject.Code}] {uniqueFileName} — upload OK nhưng AI không đọc được");
                    }
                    catch (Exception ex)
                    {
                        results.Add($"❌ [{subject.Code}] {uniqueFileName} — lỗi: {ex.Message}");
                    }
                }
            }

            TempData["SeedResults"] = results;
            return RedirectToAction("Result");
        }

        // GET /Seeder/Result — hiển thị kết quả
        [HttpGet]
        public IActionResult Result()
        {
            ViewBag.Results = TempData["SeedResults"] as List<string> ?? new List<string>();
            return View();
        }

        // ─────────────────────────────────────────────────────────
        // NỘI DUNG TÀI LIỆU IT
        // ─────────────────────────────────────────────────────────
        private static List<(string FileName, string Content)> GetDocumentTemplates()
        {
            return new List<(string, string)>
            {
                ("machine-learning-co-ban.txt",       DocMachineLearning()),
                ("cau-truc-du-lieu-giai-thuat.txt",   DocDataStructures()),
                ("lap-trinh-huong-doi-tuong.txt",     DocOOP()),
                ("bao-mat-ung-dung-web.txt",          DocWebSecurity()),
                ("co-so-du-lieu-quan-he.txt",         DocDatabase()),
                ("dien-toan-dam-may.txt",             DocCloudComputing()),
                ("phat-trien-web-hien-dai.txt",       DocWebDev()),
                ("git-quan-ly-phien-ban.txt",         DocGit()),
                ("mang-may-tinh-co-ban.txt",          DocNetworking()),
            };
        }

        private static string DocMachineLearning() => @"
GIỚI THIỆU VỀ MACHINE LEARNING

Machine Learning (Học máy) là một nhánh của Trí tuệ nhân tạo (AI), cho phép máy tính học từ dữ liệu mà không cần lập trình tường minh cho từng tình huống. Thay vì viết quy tắc thủ công, chúng ta cung cấp dữ liệu cho mô hình và để mô hình tự tìm ra quy luật.

CÁC LOẠI MACHINE LEARNING

1. Học có giám sát (Supervised Learning)
Mô hình học từ tập dữ liệu đã được gán nhãn. Ví dụ: phân loại email spam/không spam, dự đoán giá nhà.
- Classification (Phân loại): đầu ra là nhãn rời rạc (spam/không spam, mèo/chó).
- Regression (Hồi quy): đầu ra là giá trị liên tục (giá nhà, nhiệt độ ngày mai).
Các thuật toán phổ biến: Linear Regression, Logistic Regression, Decision Tree, Random Forest, SVM.

2. Học không giám sát (Unsupervised Learning)
Mô hình tự tìm cấu trúc ẩn trong dữ liệu chưa có nhãn.
- Clustering (Phân cụm): nhóm các điểm dữ liệu tương tự nhau (K-Means, DBSCAN).
- Dimensionality Reduction: giảm chiều dữ liệu (PCA, t-SNE) để trực quan hóa hoặc tăng tốc huấn luyện.

3. Học tăng cường (Reinforcement Learning)
Tác nhân (agent) học thông qua tương tác với môi trường, nhận phần thưởng/phạt. Ứng dụng: game AI (AlphaGo), robot tự hành, tối ưu quảng cáo.

NEURAL NETWORK VÀ DEEP LEARNING

Mạng nơ-ron nhân tạo (Artificial Neural Network) lấy cảm hứng từ não người, gồm các lớp:
- Input Layer: nhận dữ liệu đầu vào.
- Hidden Layers: xử lý và trích xuất đặc trưng (feature extraction).
- Output Layer: trả về kết quả dự đoán.

Deep Learning sử dụng nhiều hidden layer, cho phép học các đặc trưng phức tạp. Các kiến trúc nổi bật:
- CNN (Convolutional Neural Network): nhận diện ảnh, video.
- RNN / LSTM: xử lý chuỗi, ngôn ngữ tự nhiên, time-series.
- Transformer: nền tảng của GPT, BERT, Gemini.

QUY TRÌNH XÂY DỰNG MÔ HÌNH ML

Bước 1 — Thu thập và tiền xử lý dữ liệu (Data Collection & Preprocessing)
Dữ liệu thực tế thường thiếu, nhiễu, không đồng nhất. Các bước cần thiết:
- Xử lý giá trị thiếu (imputation hoặc loại bỏ).
- Chuẩn hóa/normalization (Min-Max, Z-score).
- Mã hóa biến phân loại (One-Hot Encoding, Label Encoding).

Bước 2 — Chia tập dữ liệu
Chia thành Train / Validation / Test (thường theo tỷ lệ 70/15/15 hoặc 80/20).
Tập Test tuyệt đối không được nhìn trước trong quá trình huấn luyện.

Bước 3 — Huấn luyện và đánh giá
Các chỉ số đánh giá:
- Accuracy, Precision, Recall, F1-Score (phân loại).
- MAE, RMSE, R² (hồi quy).
- Confusion Matrix để phân tích lỗi chi tiết.

Bước 4 — Tinh chỉnh siêu tham số (Hyperparameter Tuning)
Grid Search, Random Search, Bayesian Optimization giúp tìm cấu hình tốt nhất.

Bước 5 — Triển khai (Deployment)
Đóng gói mô hình bằng ONNX, TensorFlow SavedModel, hoặc Pickle. Phục vụ qua REST API (FastAPI, Flask, TorchServe).

BIAS VÀ VARIANCE

Hai nguồn sai số chính:
- High Bias (Underfitting): mô hình quá đơn giản, không nắm bắt được quy luật. Giải pháp: tăng độ phức tạp mô hình, thêm đặc trưng.
- High Variance (Overfitting): mô hình học thuộc dữ liệu train, kém tổng quát. Giải pháp: Regularization (L1/L2), Dropout, tăng dữ liệu (Data Augmentation), Early Stopping.

ỨNG DỤNG THỰC TẾ

Machine Learning hiện diện trong nhiều lĩnh vực:
- Y tế: phát hiện ung thư qua ảnh X-quang, dự đoán tái phát bệnh.
- Tài chính: phát hiện gian lận giao dịch, chấm điểm tín dụng.
- Thương mại điện tử: hệ thống gợi ý sản phẩm (Collaborative Filtering, Content-Based).
- Xử lý ngôn ngữ tự nhiên: chatbot, dịch thuật tự động, tóm tắt văn bản.
- Computer Vision: nhận diện khuôn mặt, xe tự lái, kiểm tra chất lượng sản xuất.

Với sự phát triển của các framework như TensorFlow, PyTorch, scikit-learn và nền tảng cloud như Google Vertex AI, Amazon SageMaker, việc xây dựng và triển khai mô hình ML ngày càng trở nên dễ tiếp cận hơn với lập trình viên.
".Trim();

        private static string DocDataStructures() => @"
CẤU TRÚC DỮ LIỆU VÀ GIẢI THUẬT

Cấu trúc dữ liệu (Data Structure) và Giải thuật (Algorithm) là nền tảng của khoa học máy tính. Việc chọn đúng cấu trúc dữ liệu và giải thuật ảnh hưởng trực tiếp đến hiệu năng và khả năng bảo trì của phần mềm.

ĐỘ PHỨC TẠP THUẬT TOÁN — BIG O NOTATION

Big O Notation mô tả tốc độ tăng trưởng thời gian/bộ nhớ theo kích thước đầu vào n:
- O(1) — Hằng số: truy cập phần tử mảng theo chỉ số.
- O(log n) — Logarithm: tìm kiếm nhị phân (Binary Search).
- O(n) — Tuyến tính: duyệt toàn bộ danh sách.
- O(n log n) — Merge Sort, Quick Sort trung bình.
- O(n²) — Bubble Sort, Selection Sort, vòng lặp lồng nhau.
- O(2ⁿ) — Bài toán tập hợp con, bài toán người du lịch (brute force).

Mục tiêu luôn là giảm độ phức tạp về thời gian và không gian.

CÁC CẤU TRÚC DỮ LIỆU CƠ BẢN

1. Mảng (Array)
Lưu trữ các phần tử cùng kiểu liên tiếp trong bộ nhớ.
- Truy cập O(1), thêm cuối O(1), thêm giữa O(n), tìm kiếm O(n).
- Phù hợp khi kích thước cố định và cần truy cập ngẫu nhiên.

2. Danh sách liên kết (Linked List)
Mỗi node chứa dữ liệu và con trỏ đến node tiếp theo.
- Thêm/xóa đầu O(1), truy cập O(n).
- Singly Linked List, Doubly Linked List, Circular Linked List.

3. Ngăn xếp (Stack) — LIFO (Last In, First Out)
Các thao tác: push (thêm), pop (lấy ra), peek (xem đỉnh).
Ứng dụng: quản lý lời gọi hàm (call stack), undo/redo, kiểm tra dấu ngoặc hợp lệ.

4. Hàng đợi (Queue) — FIFO (First In, First Out)
Thao tác: enqueue (thêm cuối), dequeue (lấy đầu).
Biến thể: Priority Queue (ưu tiên), Deque (hai đầu), Circular Queue.
Ứng dụng: lập lịch CPU, BFS, hàng đợi in ấn.

5. Bảng băm (Hash Table)
Ánh xạ key → value qua hàm hash.
- Thêm, xóa, tìm kiếm trung bình O(1).
- Giải quyết đụng độ: Chaining (danh sách liên kết) hoặc Open Addressing.
Ứng dụng: từ điển, bộ nhớ đệm (cache), đếm tần suất.

6. Cây (Tree)
Cấu trúc phân cấp với một gốc (root) và các node con.

Binary Search Tree (BST): node trái < gốc < node phải.
- Tìm kiếm, thêm, xóa O(log n) nếu cân bằng.

AVL Tree / Red-Black Tree: BST tự cân bằng, đảm bảo O(log n).

Heap: cây nhị phân hoàn chỉnh, dùng cho Priority Queue và Heap Sort.
- Max-Heap: gốc là phần tử lớn nhất.
- Min-Heap: gốc là phần tử nhỏ nhất.

Trie: cây tiền tố, tối ưu tìm kiếm chuỗi (autocomplete, spell-check).

7. Đồ thị (Graph)
Gồm tập đỉnh (vertices) và cạnh (edges). Có hướng hoặc vô hướng, có trọng số hoặc không.
Biểu diễn: ma trận kề (Adjacency Matrix) hoặc danh sách kề (Adjacency List).

CÁC GIẢI THUẬT QUAN TRỌNG

Sắp xếp (Sorting):
- Bubble Sort: O(n²) — đơn giản, chậm.
- Merge Sort: O(n log n) — ổn định, dùng chia để trị.
- Quick Sort: O(n log n) trung bình — nhanh trong thực tế, không ổn định.
- Counting Sort / Radix Sort: O(n + k) — tối ưu cho dữ liệu số nguyên giới hạn.

Tìm kiếm (Searching):
- Linear Search: O(n) — dùng khi dữ liệu chưa sắp xếp.
- Binary Search: O(log n) — yêu cầu dữ liệu đã sắp xếp.

Duyệt đồ thị:
- BFS (Breadth-First Search): dùng Queue, tìm đường ngắn nhất trong đồ thị không trọng số.
- DFS (Depth-First Search): dùng Stack/đệ quy, phát hiện chu trình, topological sort.
- Dijkstra: đường đi ngắn nhất trong đồ thị có trọng số không âm.
- A* Search: tối ưu Dijkstra với heuristic, dùng trong pathfinding game/bản đồ.

Chia để trị (Divide & Conquer): Merge Sort, Binary Search, Fast Fourier Transform.
Quy hoạch động (Dynamic Programming): Fibonacci, Knapsack, Longest Common Subsequence.
Tham lam (Greedy): Huffman Coding, Activity Selection, Kruskal/Prim (cây khung nhỏ nhất).

CHỌN CẤU TRÚC DỮ LIỆU PHÙ HỢP

Hiệu năng phần mềm phụ thuộc lớn vào lựa chọn này:
- Cần truy cập ngẫu nhiên nhanh → Array / HashMap.
- Cần thêm/xóa đầu/cuối thường xuyên → LinkedList / Deque.
- Cần thứ tự ưu tiên → Heap / Priority Queue.
- Cần tìm kiếm theo phạm vi → BST / Sorted Array.
- Cần tìm kiếm text → Trie / Inverted Index.
- Cần mô hình mạng, quan hệ → Graph.
".Trim();

        private static string DocOOP() => @"
LẬP TRÌNH HƯỚNG ĐỐI TƯỢNG (OOP)

Lập trình hướng đối tượng (Object-Oriented Programming — OOP) là mô hình lập trình tổ chức code thành các đối tượng kết hợp dữ liệu (thuộc tính) và hành vi (phương thức). OOP giúp code dễ bảo trì, mở rộng và tái sử dụng.

BỐN TÍNH CHẤT CỦA OOP

1. Đóng gói (Encapsulation)
Che giấu trạng thái nội bộ, chỉ để lộ giao diện cần thiết ra bên ngoài.
- Dữ liệu được đánh dấu private/protected, truy cập qua getter/setter.
- Lợi ích: kiểm soát truy cập, giảm phụ thuộc giữa các module, dễ thay đổi triển khai nội bộ mà không ảnh hưởng client code.

2. Kế thừa (Inheritance)
Lớp con (subclass) thừa hưởng thuộc tính và phương thức từ lớp cha (superclass).
- Tái sử dụng code, mở rộng chức năng mà không sửa lớp gốc.
- Lưu ý: ưu tiên Composition over Inheritance để tránh tầng kế thừa sâu, khó bảo trì.

3. Đa hình (Polymorphism)
Cùng một giao diện, nhiều cách triển khai khác nhau.
- Compile-time (static): Method Overloading — cùng tên hàm, khác tham số.
- Runtime (dynamic): Method Overriding — lớp con ghi đè phương thức lớp cha.
Lợi ích: code linh hoạt, dễ mở rộng, phù hợp với nguyên lý Open/Closed.

4. Trừu tượng hóa (Abstraction)
Ẩn đi sự phức tạp, chỉ để lộ những gì cần thiết.
- Abstract Class: có thể có phương thức trừu tượng và cài đặt mặc định.
- Interface: chỉ định nghĩa hợp đồng (contract), không có triển khai (trừ default method trong Java 8+).

NGUYÊN LÝ SOLID

S — Single Responsibility Principle (SRP)
Mỗi class chỉ có một lý do để thay đổi. Một class không nên đảm nhận quá nhiều trách nhiệm.

O — Open/Closed Principle (OCP)
Class nên mở để mở rộng (extension) nhưng đóng với việc sửa đổi (modification). Dùng interface và abstract class để đạt được điều này.

L — Liskov Substitution Principle (LSP)
Đối tượng lớp con phải có thể thay thế lớp cha mà không làm hỏng chương trình. Lớp con không được vi phạm hợp đồng của lớp cha.

I — Interface Segregation Principle (ISP)
Không nên ép client implement những interface mà họ không cần. Tách interface lớn thành nhiều interface nhỏ hơn, chuyên biệt hơn.

D — Dependency Inversion Principle (DIP)
Module cấp cao không nên phụ thuộc vào module cấp thấp. Cả hai nên phụ thuộc vào abstraction (interface). Đây là nền tảng của Dependency Injection (DI).

DESIGN PATTERNS PHỔ BIẾN

Creational Patterns (tạo đối tượng):
- Singleton: đảm bảo chỉ có một instance duy nhất (Logger, Config).
- Factory Method: tạo đối tượng qua factory thay vì new trực tiếp.
- Builder: tạo đối tượng phức tạp từng bước (QueryBuilder, StringBuilder).

Structural Patterns (cấu trúc):
- Adapter: chuyển đổi interface không tương thích.
- Decorator: thêm chức năng vào đối tượng mà không sửa class gốc.
- Repository: trừu tượng hóa tầng truy cập dữ liệu.
- Facade: cung cấp interface đơn giản cho hệ thống phức tạp.

Behavioral Patterns (hành vi):
- Observer: pub-sub, event-driven (DOM events, message queue).
- Strategy: đổi thuật toán tại runtime (thuật toán sắp xếp, thanh toán).
- Command: đóng gói yêu cầu thành đối tượng (undo/redo, job queue).

OOP VÀ CÁC NGÔN NGỮ HIỆN ĐẠI

- Java, C#: OOP thuần túy, mọi thứ đều là class.
- Python: hỗ trợ OOP linh hoạt, không ép buộc.
- JavaScript/TypeScript: prototype-based, ES6+ thêm cú pháp class.
- Go: không có class truyền thống, dùng struct + interface.
- Rust: struct + trait, không có inheritance.

Xu hướng hiện đại kết hợp OOP với Functional Programming: immutability, pure functions, first-class functions giúp giảm side effects và tăng khả năng kiểm thử.
".Trim();

        private static string DocWebSecurity() => @"
BẢO MẬT ỨNG DỤNG WEB

Bảo mật là yêu cầu bắt buộc trong phát triển phần mềm. OWASP (Open Web Application Security Project) công bố danh sách Top 10 lỗ hổng phổ biến nhất, cập nhật định kỳ, là tài liệu tham chiếu tiêu chuẩn trong ngành.

OWASP TOP 10 (2021)

1. Broken Access Control
Người dùng có thể truy cập tài nguyên ngoài quyền hạn.
Phòng chống: kiểm tra phân quyền ở server-side, không chỉ ẩn UI phía client; dùng nguyên tắc tối thiểu quyền (Principle of Least Privilege).

2. Cryptographic Failures
Dữ liệu nhạy cảm không được mã hóa hoặc dùng thuật toán yếu.
Phòng chống: dùng AES-256, RSA-2048+; không dùng MD5/SHA-1 cho password (dùng bcrypt, Argon2, PBKDF2); TLS 1.2+ cho mọi kết nối.

3. SQL Injection (SQLi)
Kẻ tấn công chèn SQL độc hại vào input để truy vấn hoặc xóa dữ liệu.
Ví dụ: input ' OR '1'='1 khiến WHERE clause luôn đúng.
Phòng chống: Parameterized Query / Prepared Statement, không nối chuỗi SQL trực tiếp, dùng ORM (Entity Framework, Hibernate).

4. Insecure Design
Thiếu threat modeling, thiếu kiểm soát nghiệp vụ quan trọng.
Phòng chống: Secure by Design, thực hiện threat modeling (STRIDE), kiểm tra luồng nghiệp vụ (rate limiting, captcha).

5. Security Misconfiguration
Cấu hình mặc định không an toàn, để lộ stack trace, debug mode bật production.
Phòng chống: disable các tính năng không dùng, cấu hình security headers, xóa tài khoản mặc định.

6. Vulnerable and Outdated Components
Dùng thư viện, framework có lỗ hổng đã biết.
Phòng chống: kiểm tra CVE database (NVD, Snyk), cập nhật dependency thường xuyên, dùng Software Composition Analysis (SCA).

7. XSS — Cross-Site Scripting
Chèn JavaScript độc hại vào trang web để chạy trong browser nạn nhân.
- Reflected XSS: payload trong URL.
- Stored XSS: payload lưu vào DB, ảnh hưởng nhiều người dùng.
- DOM-based XSS: xử lý bằng JavaScript phía client.
Phòng chống: encode HTML output (htmlspecialchars), Content Security Policy (CSP), HttpOnly cookies.

8. Insecure Deserialization
Deserialize dữ liệu không tin cậy dẫn đến Remote Code Execution.
Phòng chống: không deserialize dữ liệu từ nguồn không tin cậy, dùng JSON thay vì binary serialization.

9. Security Logging and Monitoring Failures
Không ghi log đầy đủ, không phát hiện tấn công kịp thời.
Phòng chống: log authentication failures, phân quyền thất bại; tích hợp SIEM; thiết lập alert.

10. Server-Side Request Forgery (SSRF)
Server thực hiện request đến địa chỉ nội bộ theo yêu cầu của kẻ tấn công.
Phòng chống: validate và whitelist URL, không cho server fetch URL tùy ý từ user input.

XÁC THỰC VÀ PHÂN QUYỀN

Authentication (Xác thực — Bạn là ai?):
- Password-based: cần hash mạnh (bcrypt, Argon2), salt ngẫu nhiên.
- Multi-Factor Authentication (MFA): OTP (TOTP/HOTP), hardware key (FIDO2/WebAuthn).
- OAuth 2.0 + OpenID Connect: đăng nhập bằng Google, Facebook, GitHub.
- JWT (JSON Web Token): stateless token, cần kiểm tra chữ ký (RS256 tốt hơn HS256 trong multi-service).

Authorization (Phân quyền — Bạn được làm gì?):
- RBAC (Role-Based Access Control): phân quyền theo vai trò.
- ABAC (Attribute-Based): phân quyền theo thuộc tính động (thời gian, địa điểm, dữ liệu).
- Kiểm tra ở server-side, không tin tưởng kiểm tra phía client.

BẢO VỆ DỮ LIỆU TRUYỀN TẢI

HTTPS/TLS:
- Sử dụng TLS 1.2 hoặc 1.3, vô hiệu hóa SSL 2.0/3.0, TLS 1.0/1.1.
- Certificate Transparency, HSTS (HTTP Strict Transport Security).
- Certificate Pinning cho mobile apps.

Security Headers quan trọng:
- Content-Security-Policy (CSP): kiểm soát nguồn tài nguyên được tải.
- X-Frame-Options: chống Clickjacking.
- X-Content-Type-Options: nosniff.
- Referrer-Policy: kiểm soát thông tin referrer.
- Permissions-Policy: giới hạn quyền truy cập API (camera, microphone).

DEVSECOPS — BẢO MẬT TRONG QUY TRÌNH PHÁT TRIỂN

Tích hợp bảo mật vào pipeline CI/CD:
- SAST (Static Application Security Testing): phân tích code tĩnh (SonarQube, Semgrep).
- DAST (Dynamic Application Security Testing): kiểm tra ứng dụng đang chạy (OWASP ZAP, Burp Suite).
- Dependency Scanning: phát hiện thư viện có lỗ hổng (Snyk, Dependabot).
- Container Scanning: kiểm tra image Docker (Trivy, Clair).
- Penetration Testing: kiểm tra xâm nhập định kỳ bởi chuyên gia.
".Trim();

        private static string DocDatabase() => @"
CƠ SỞ DỮ LIỆU QUAN HỆ

Cơ sở dữ liệu quan hệ (Relational Database) tổ chức dữ liệu dưới dạng bảng (table) gồm hàng (row/record) và cột (column/field). Các bảng liên kết với nhau qua khóa ngoại (foreign key). Các hệ quản trị phổ biến: PostgreSQL, MySQL, SQL Server, Oracle.

MÔ HÌNH THỰC THỂ — QUAN HỆ (ER DIAGRAM)

Trước khi thiết kế cơ sở dữ liệu, cần xây dựng ER Diagram:
- Entity (Thực thể): đối tượng cần lưu trữ (Sinh viên, Môn học, Đơn hàng).
- Attribute (Thuộc tính): thông tin của thực thể (Tên, Ngày sinh, Email).
- Relationship (Quan hệ): liên kết giữa các thực thể (Sinh viên đăng ký Môn học).
- Cardinality: 1-1, 1-N (one-to-many), N-N (many-to-many).

Quan hệ N-N thường được chuyển thành bảng trung gian (junction table) chứa khóa ngoại của cả hai bảng.

CHUẨN HÓA DỮ LIỆU (NORMALIZATION)

Mục đích: loại bỏ dữ liệu trùng lặp, đảm bảo tính toàn vẹn.

1NF (First Normal Form):
- Mỗi cột chứa giá trị nguyên tử (atomic).
- Không có cột đa trị (multi-valued) hay cột lặp.

2NF (Second Normal Form):
- Thỏa 1NF.
- Mọi cột non-key phụ thuộc hoàn toàn vào toàn bộ Primary Key (loại bỏ phụ thuộc bộ phận — partial dependency).

3NF (Third Normal Form):
- Thỏa 2NF.
- Không có phụ thuộc bắc cầu (transitive dependency): cột A → cột B → cột C. Cần tách bảng.

BCNF (Boyce-Codd Normal Form): phiên bản chặt chẽ hơn 3NF, đảm bảo mọi determinant đều là Superkey.

SQL CƠ BẢN

DDL (Data Definition Language):
  CREATE TABLE, ALTER TABLE, DROP TABLE, CREATE INDEX.

DML (Data Manipulation Language):
  INSERT INTO, SELECT, UPDATE, DELETE.

Các câu truy vấn quan trọng:
- JOIN: INNER JOIN (giao), LEFT JOIN, RIGHT JOIN, FULL OUTER JOIN.
- Aggregation: COUNT, SUM, AVG, MAX, MIN kết hợp GROUP BY.
- Subquery và Common Table Expression (CTE) với WITH.
- Window Function: ROW_NUMBER, RANK, LEAD, LAG — phân tích dữ liệu trong nhóm.

Ví dụ CTE:
  WITH RankedSales AS (
    SELECT ProductId, SUM(Amount) AS Total,
           RANK() OVER (ORDER BY SUM(Amount) DESC) AS Rnk
    FROM Orders GROUP BY ProductId
  )
  SELECT * FROM RankedSales WHERE Rnk <= 10;

INDEX VÀ TỐI ƯU TRUY VẤN

Index (Chỉ mục): cấu trúc B-Tree giúp tăng tốc tìm kiếm, đánh đổi tốc độ ghi.
- Nên đánh index trên cột thường dùng trong WHERE, JOIN, ORDER BY.
- Composite Index: index nhiều cột, thứ tự cột quan trọng.
- Partial Index: index theo điều kiện (PostgreSQL).
- Full-Text Index: tìm kiếm văn bản (tsvector trong PostgreSQL).

Query Optimization:
- Dùng EXPLAIN / EXPLAIN ANALYZE để phân tích execution plan.
- Tránh SELECT *, chỉ lấy cột cần thiết.
- Tránh N+1 query — dùng JOIN hoặc eager loading.
- Sử dụng Materialized View cho dữ liệu tổng hợp phức tạp.

ACID PROPERTIES

Mọi transaction trong RDBMS phải đảm bảo:
- Atomicity: toàn bộ transaction thành công hoặc rollback hoàn toàn.
- Consistency: dữ liệu luôn ở trạng thái hợp lệ sau mỗi transaction.
- Isolation: các transaction song song không ảnh hưởng nhau (mức độ: Read Uncommitted, Read Committed, Repeatable Read, Serializable).
- Durability: dữ liệu đã commit được lưu bền vững, ngay cả khi hệ thống sự cố.

NoSQL VÀ KỊCH BẢN SỬ DỤNG

Khi nên dùng NoSQL thay vì SQL:
- Document DB (MongoDB): dữ liệu không có cấu trúc cố định, JSON linh hoạt.
- Key-Value (Redis): cache, session, rate limiting — hiệu năng cực cao.
- Column-Family (Cassandra): ghi nhiều, phân tán địa lý, time-series.
- Graph DB (Neo4j): quan hệ phức tạp (mạng xã hội, gợi ý sản phẩm).

Xu hướng NewSQL (CockroachDB, TiDB) kết hợp ACID của SQL với khả năng mở rộng ngang của NoSQL.
".Trim();

        private static string DocCloudComputing() => @"
ĐIỆN TOÁN ĐÁM MÂY (CLOUD COMPUTING)

Điện toán đám mây là mô hình cung cấp tài nguyên IT (máy chủ, lưu trữ, mạng, phần mềm) qua internet, theo nhu cầu, thanh toán theo mức sử dụng (pay-as-you-go). Thay vì mua phần cứng, doanh nghiệp thuê tài nguyên từ cloud provider.

MÔ HÌNH DỊCH VỤ CLOUD

IaaS — Infrastructure as a Service:
Cung cấp hạ tầng cơ bản: máy ảo, storage, network. Người dùng tự quản lý OS, middleware, ứng dụng.
Ví dụ: Amazon EC2, Google Compute Engine, Azure Virtual Machines.
Phù hợp: migration từ on-premise, cần kiểm soát tối đa môi trường.

PaaS — Platform as a Service:
Cung cấp nền tảng phát triển và triển khai ứng dụng. Nhà cung cấp quản lý OS, runtime, middleware.
Ví dụ: Google App Engine, Azure App Service, Heroku, Fly.io.
Phù hợp: team muốn tập trung vào code, không muốn quản lý server.

SaaS — Software as a Service:
Phần mềm sẵn dùng qua trình duyệt. Người dùng không cần cài đặt hay bảo trì.
Ví dụ: Gmail, Microsoft 365, Salesforce, Slack, Figma.
Phù hợp: người dùng cuối, doanh nghiệp vừa và nhỏ.

FaaS — Function as a Service (Serverless):
Chạy hàm theo sự kiện, không cần quản lý server. Tính phí theo lần gọi và thời gian thực thi.
Ví dụ: AWS Lambda, Google Cloud Functions, Azure Functions.
Phù hợp: xử lý event-driven, batch job, microservice nhỏ, biến động tải lớn.

MÔ HÌNH TRIỂN KHAI

Public Cloud: tài nguyên được chia sẻ giữa nhiều khách hàng (multi-tenant). Chi phí thấp, linh hoạt cao.
Private Cloud: hạ tầng riêng cho một tổ chức, tự vận hành hoặc thuê cơ sở.
Hybrid Cloud: kết hợp public và private, dữ liệu nhạy cảm ở private, workload biến động ở public.
Multi-Cloud: sử dụng nhiều cloud provider (AWS + GCP + Azure) để tránh vendor lock-in và tăng dự phòng.

CÁC DỊCH VỤ QUAN TRỌNG TRÊN AWS / GCP / AZURE

Compute: EC2 / Compute Engine / Virtual Machines.
Object Storage: S3 / Cloud Storage / Blob Storage.
Relational DB: RDS / Cloud SQL / Azure SQL Database.
Serverless: Lambda / Cloud Functions / Azure Functions.
Container Orchestration: EKS / GKE / AKS (đều dùng Kubernetes).
AI/ML: SageMaker / Vertex AI / Azure Machine Learning.
CDN: CloudFront / Cloud CDN / Azure CDN.

CONTAINERIZATION VÀ KUBERNETES

Docker:
- Đóng gói ứng dụng cùng dependencies vào image bất biến.
- Chạy nhất quán trên mọi môi trường (local, staging, production).
- Dockerfile định nghĩa cách build image; docker-compose quản lý multi-container.

Kubernetes (K8s):
- Orchestrate container ở quy mô lớn.
- Pod: đơn vị triển khai nhỏ nhất (1+ container).
- Deployment: quản lý lifecycle của Pod, rolling update, rollback.
- Service: expose Pod ra ngoài qua ClusterIP, NodePort, LoadBalancer.
- ConfigMap / Secret: tách cấu hình khỏi image.
- HPA (Horizontal Pod Autoscaler): tự động scale theo CPU/memory/custom metrics.

ĐỘ TIN CẬY VÀ TỐI ƯU CHI PHÍ

SLA (Service Level Agreement): AWS, GCP, Azure cam kết uptime 99.9% - 99.99% cho các dịch vụ managed.

Cloud Cost Optimization:
- Reserved Instances / Committed Use Discounts: tiết kiệm 30-70% so với on-demand.
- Spot Instances (AWS) / Preemptible VMs (GCP): giảm 70-90%, phù hợp workload fault-tolerant.
- Right-sizing: chọn instance type phù hợp, không over-provision.
- Auto Scaling: tự động tăng/giảm tài nguyên theo tải.
- FinOps: văn hóa tối ưu chi phí cloud trong tổ chức (Tagging, Cost Allocation, Budget Alert).

Cloud-Native Architecture:
- Microservices: chia ứng dụng thành service nhỏ, triển khai độc lập.
- Event-Driven: giao tiếp qua message queue (Kafka, SQS, Pub/Sub).
- Immutable Infrastructure: không sửa server running, tạo image mới khi cần thay đổi.
- Infrastructure as Code (IaC): Terraform, Pulumi, CloudFormation để quản lý hạ tầng bằng code.
".Trim();

        private static string DocWebDev() => @"
PHÁT TRIỂN WEB HIỆN ĐẠI

Web development ngày nay bao gồm frontend (giao diện người dùng), backend (logic nghiệp vụ và API), và các công nghệ hỗ trợ triển khai, bảo mật, hiệu năng.

HTML5 — CẤU TRÚC TRANG WEB

HTML5 bổ sung nhiều thẻ ngữ nghĩa (semantic elements):
- <header>, <nav>, <main>, <article>, <section>, <aside>, <footer>: cải thiện SEO và accessibility.
- <figure>, <figcaption>: hình ảnh với chú thích.
- <video>, <audio>: nhúng media mà không cần plugin.
- <canvas>: vẽ đồ họa 2D bằng JavaScript.
- <dialog>: dialog box native.
- Thuộc tính: data-*, aria-* (accessibility), autocomplete, placeholder, required, pattern.

HTML5 APIs quan trọng:
- LocalStorage / SessionStorage: lưu dữ liệu phía client.
- Web Workers: chạy JS ở background thread, không block UI.
- WebSocket: kết nối hai chiều real-time.
- Geolocation, Notifications, Clipboard API.

CSS3 — TRÌNH BÀY VÀ HOẠT ẢNH

Layout hiện đại:
- Flexbox: căn chỉnh một chiều (row hoặc column), rất phù hợp cho component.
- CSS Grid: layout hai chiều, tạo bố cục phức tạp dễ dàng.
- Container Queries: responsive dựa trên kích thước container, không chỉ viewport.

CSS Variables (Custom Properties):
  :root { --primary-color: #6366F1; --spacing: 16px; }
  button { background: var(--primary-color); padding: var(--spacing); }

Animation và Transition:
- transition: hover effects, state changes mượt mà.
- animation + @keyframes: hoạt ảnh phức tạp không cần JavaScript.

CSS Preprocessors (Sass/Less) và Utility-first CSS (Tailwind CSS) tăng năng suất viết style.

JAVASCRIPT HIỆN ĐẠI (ES6+)

Các tính năng quan trọng:
- Arrow functions: cú pháp ngắn gọn, không có this riêng.
- Destructuring: const { name, age } = user;
- Spread/Rest: ...args, ...array
- Template literals: `Xin chào, ${name}!`
- Promises và async/await: xử lý bất đồng bộ dễ đọc hơn callback.
- Optional Chaining (?.) và Nullish Coalescing (??) — ES2020.
- Modules (import/export): tổ chức code theo file.

TypeScript: superset của JavaScript, thêm static typing, interface, generics. Giảm lỗi runtime, cải thiện DX (Developer Experience) với IDE support tốt hơn.

CÁC FRAMEWORK FRONTEND PHỔ BIẾN

React (Meta): thư viện UI, Virtual DOM, component-based. Hệ sinh thái lớn nhất.
Vue.js: progressive framework, cú pháp thân thiện, Composition API.
Angular (Google): full framework, TypeScript mặc định, DI tích hợp, phù hợp enterprise.
Svelte: compile-time framework, không runtime overhead, code ngắn gọn.
Next.js / Nuxt.js / SvelteKit: full-stack framework với SSR, SSG, ISR.

REST API VÀ GRAPHQL

REST API:
- Dùng HTTP methods đúng nghĩa: GET (lấy), POST (tạo), PUT/PATCH (cập nhật), DELETE (xóa).
- Status codes: 200 OK, 201 Created, 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found, 500 Internal Server Error.
- Versioning: /api/v1/users
- HATEOAS: response chứa link đến resource liên quan.

GraphQL:
- Client chỉ lấy đúng dữ liệu cần (no over-fetching/under-fetching).
- Single endpoint /graphql.
- Schema strongly-typed, subscription cho real-time.
- Phù hợp khi nhiều client (web, mobile) cần dữ liệu khác nhau.

HIỆU NĂNG WEB

Core Web Vitals (Google):
- LCP (Largest Contentful Paint): < 2.5s — tốc độ tải nội dung chính.
- FID/INP (Interaction to Next Paint): < 200ms — phản hồi tương tác.
- CLS (Cumulative Layout Shift): < 0.1 — ổn định layout.

Kỹ thuật tối ưu:
- Lazy loading image và component.
- Code splitting và tree shaking (chỉ bundle code cần dùng).
- CDN cho static assets.
- HTTP/2 và HTTP/3 (QUIC) giảm latency.
- Service Worker và PWA (Progressive Web App) cho offline support.
- Server-Side Rendering (SSR) và Static Generation (SSG) cho SEO và TTFB tốt hơn.
".Trim();

        private static string DocGit() => @"
GIT VÀ QUẢN LÝ PHIÊN BẢN

Git là hệ thống kiểm soát phiên bản phân tán (Distributed Version Control System — DVCS) được tạo bởi Linus Torvalds năm 2005. Mỗi developer có toàn bộ lịch sử repository trên máy mình, làm việc offline được và merge khi cần.

CÁC KHÁI NIỆM CỐT LÕI

Repository (Repo): kho chứa toàn bộ code và lịch sử thay đổi.
Working Directory: thư mục làm việc hiện tại trên máy.
Staging Area (Index): vùng trung gian, chứa thay đổi chuẩn bị commit.
Commit: snapshot của code tại một thời điểm, có hash SHA-1 duy nhất.
Branch: con trỏ nhẹ trỏ đến một commit. Tạo branch O(1), không tốn kém.
HEAD: con trỏ đến commit hiện tại (thường là tip của branch hiện tại).
Remote: repository từ xa (GitHub, GitLab, Bitbucket).

LUỒNG LÀM VIỆC CƠ BẢN

git init hoặc git clone <url>: khởi tạo hoặc sao chép repo.
git status: xem trạng thái working directory.
git add <file> hoặc git add .: stage thay đổi.
git commit -m ""message"": tạo commit với message mô tả.
git push origin <branch>: đẩy code lên remote.
git pull: fetch + merge thay đổi từ remote.
git fetch: tải thay đổi từ remote nhưng không merge.

GIT BRANCHING STRATEGY

Feature Branch Workflow:
- Mỗi tính năng/bugfix làm trên branch riêng (feature/login, fix/auth-bug).
- Tạo Pull Request / Merge Request để review code trước khi merge.
- Xóa branch sau khi merge.

Gitflow:
- main/master: code production, luôn ổn định.
- develop: tích hợp tính năng, base cho release.
- feature/*: phát triển tính năng mới từ develop.
- release/*: chuẩn bị phát hành (bugfix, versioning).
- hotfix/*: vá lỗi khẩn trên production, merge vào cả main và develop.

Trunk-Based Development:
- Tất cả commit thẳng vào trunk (main). Branch tồn tại < 1-2 ngày.
- Dùng Feature Flags để ẩn tính năng chưa sẵn sàng.
- Phù hợp với CI/CD nhanh, team có kỷ luật cao.

GitHub Flow: đơn giản hơn Gitflow — chỉ có main và feature branches, deploy trực tiếp từ main.

MERGE VÀ REBASE

Merge: gộp hai nhánh, tạo merge commit. Lịch sử nguyên vẹn, dễ trace.
  git merge feature/login

Rebase: di chuyển commit của branch lên đỉnh target branch. Lịch sử tuyến tính, sạch hơn.
  git rebase develop
  Lưu ý: không rebase branch đã push lên remote mà người khác đang dùng (golden rule of rebase).

Squash: gộp nhiều commit nhỏ thành một commit trước khi merge (git merge --squash, git rebase -i).

PULL REQUEST VÀ CODE REVIEW

Pull Request (GitHub) / Merge Request (GitLab):
- Mô tả thay đổi, lý do, cách test.
- Link đến issue/ticket liên quan.
- Checklist: unit test, documentation, security review.

Code Review best practices:
- Review code, không review người.
- Đặt câu hỏi thay vì ra lệnh.
- Approve khi đủ tự tin, không để PR chờ quá 24h.
- Dùng Conventional Commits: feat:, fix:, docs:, refactor:, test:, chore:.

GIT HOOKS VÀ TỰ ĐỘNG HÓA

Git hooks: script tự động chạy tại các sự kiện git (pre-commit, commit-msg, pre-push).
- pre-commit: chạy linter, format code (ESLint, Prettier, dotnet format).
- commit-msg: kiểm tra format commit message (Commitlint).
- pre-push: chạy unit test trước khi push.

Husky + lint-staged (JavaScript): tự động cài Git hooks trong dự án Node.js.

ADVANCED GIT COMMANDS

git stash: lưu tạm thay đổi chưa commit để switch branch.
git cherry-pick <hash>: áp dụng một commit cụ thể vào branch hiện tại.
git bisect: tìm commit gây ra bug bằng Binary Search.
git reflog: xem toàn bộ lịch sử HEAD, hữu ích khi cần khôi phục.
git worktree: checkout nhiều branch cùng lúc vào thư mục khác nhau.
".Trim();

        private static string DocNetworking() => @"
MẠNG MÁY TÍNH CƠ BẢN

Mạng máy tính là hệ thống kết nối các thiết bị để chia sẻ tài nguyên và truyền thông tin. Hiểu kiến trúc mạng là nền tảng cho backend developer, DevOps, và security engineer.

MÔ HÌNH OSI — 7 LỚP

Mô hình OSI (Open Systems Interconnection) phân chia chức năng mạng thành 7 lớp trừu tượng:

Lớp 7 — Application: giao tiếp với ứng dụng người dùng. Giao thức: HTTP, HTTPS, FTP, SMTP, DNS, WebSocket.
Lớp 6 — Presentation: mã hóa/giải mã, nén dữ liệu. SSL/TLS hoạt động ở đây.
Lớp 5 — Session: quản lý phiên kết nối (session establishment, maintenance, termination).
Lớp 4 — Transport: truyền tải end-to-end, kiểm soát luồng, phục hồi lỗi. Giao thức: TCP, UDP.
Lớp 3 — Network: định tuyến gói tin giữa các mạng. Giao thức: IP (IPv4/IPv6), ICMP, Routing.
Lớp 2 — Data Link: truyền frame giữa hai node trên cùng mạng. Ethernet, MAC address, ARP.
Lớp 1 — Physical: tín hiệu vật lý — cáp đồng, cáp quang, sóng vô tuyến.

Mô hình TCP/IP (thực tế) gộp thành 4 lớp: Application, Transport, Internet, Network Access.

TCP VÀ UDP

TCP (Transmission Control Protocol):
- Có kết nối (connection-oriented): bắt tay 3 bước (SYN → SYN-ACK → ACK).
- Đảm bảo thứ tự và toàn vẹn dữ liệu.
- Điều khiển tắc nghẽn và luồng (flow control, congestion control).
- Dùng cho: HTTP/HTTPS, email, database queries — mọi thứ cần độ chính xác.

UDP (User Datagram Protocol):
- Không kết nối (connectionless), không đảm bảo thứ tự hay delivery.
- Overhead thấp, latency thấp.
- Dùng cho: DNS, streaming video, VoIP, gaming, WebRTC — chấp nhận mất gói để đổi lấy tốc độ.

QUIC (HTTP/3): giao thức mới dựa trên UDP, kết hợp ưu điểm của TCP (reliability) và UDP (speed), giảm latency qua 0-RTT handshake.

HTTP VÀ HTTPS

HTTP/1.1: mỗi request một kết nối TCP (persistent connection giảm overhead nhưng vẫn HOL blocking).
HTTP/2: multiplexing — nhiều request trên một kết nối TCP, server push, header compression (HPACK).
HTTP/3: chạy trên QUIC/UDP, không HOL blocking, 0-RTT reconnect, tốt hơn trên mạng không ổn định.

HTTPS = HTTP + TLS:
- TLS Handshake: trao đổi certificate, chọn cipher suite, tạo session key.
- Mã hóa đối xứng (AES) cho data, bất đối xứng (RSA/ECDH) cho key exchange.
- Certificate Authority (CA): Let's Encrypt (miễn phí), DigiCert, Sectigo.

DNS — HỆ THỐNG TÊN MIỀN

DNS dịch domain name (google.com) thành IP address.
Quá trình phân giải:
1. Browser cache → OS cache → Router cache.
2. Recursive Resolver (ISP).
3. Root Name Server → TLD Server (.com) → Authoritative Name Server.
4. Trả về IP, cache theo TTL.

Các loại DNS record:
- A: domain → IPv4.
- AAAA: domain → IPv6.
- CNAME: alias từ domain này sang domain khác.
- MX: mail server cho domain.
- TXT: xác thực (SPF, DKIM, DMARC cho email, xác thực domain ownership).
- NS: Name Server cho domain.

BẢO MẬT MẠNG

Firewall: lọc traffic dựa trên IP, port, protocol.
- Stateless: lọc từng packet độc lập.
- Stateful: theo dõi trạng thái kết nối.
- Application Layer (WAF): hiểu HTTP, chặn SQL injection, XSS.

VPN (Virtual Private Network):
- Tạo tunnel mã hóa qua mạng công cộng.
- Site-to-site: kết nối hai văn phòng.
- Remote Access: nhân viên truy cập mạng công ty từ xa.
- WireGuard: VPN protocol hiện đại, nhanh và bảo mật hơn OpenVPN.

Zero Trust Network Access (ZTNA):
- Không tin tưởng bất kỳ ai dù trong hay ngoài mạng.
- Xác thực từng request, phân quyền tối thiểu.
- Thay thế dần VPN truyền thống trong doanh nghiệp.

Load Balancer: phân phối traffic đến nhiều server.
- Round Robin, Least Connections, IP Hash.
- Layer 4 (TCP) vs Layer 7 (HTTP — có thể route theo path, header).

CDN (Content Delivery Network): phân phối nội dung tĩnh từ server gần người dùng nhất, giảm latency và tải cho origin server.
".Trim();
    }
}
