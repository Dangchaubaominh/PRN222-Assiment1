using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RagChatbot.BLL.Helpers
{
    /// <summary>
    /// Semantic Chunker — chia văn bản theo ranh giới ngữ nghĩa tự nhiên.
    ///
    /// Chiến lược phân cấp:
    ///   1. Đoạn văn (paragraph) — ưu tiên giữ nguyên nếu đủ ngắn
    ///   2. Câu (sentence)       — không bao giờ cắt giữa câu
    ///   3. Mệnh đề             — chỉ dùng khi một câu đơn quá dài (&gt;600 từ)
    ///
    /// So với TextChunker (cắt cứng N từ):
    ///   - Mỗi chunk luôn kết thúc tại điểm dừng tự nhiên → embedding chính xác hơn
    ///   - Overlap theo câu thay vì từ → ngữ cảnh kết nối mượt hơn
    ///   - Tôn trọng cấu trúc đoạn văn (PDF, TXT, DOCX)
    /// </summary>
    public static class SemanticChunker
    {
        // Regex nhận diện kết thúc câu — hỗ trợ tiếng Việt và tiếng Anh
        // Dấu kết thúc: . ! ? … rồi theo sau là khoảng trắng + chữ hoa / số / ngoặc
        private static readonly Regex SentenceEndRegex = new Regex(
            @"(?<=[\.!?…。！？])\s+(?=\p{Lu}|\p{N}|[""\('\[《【])",
            RegexOptions.Compiled);

        // Regex tách mệnh đề trong câu quá dài (tại ; hoặc ,)
        private static readonly Regex ClauseEndRegex = new Regex(
            @"(?<=[;,])\s+",
            RegexOptions.Compiled);

        // Regex thu gọn khoảng trắng thừa
        private static readonly Regex MultiSpaceRegex = new Regex(@"[ \t]+", RegexOptions.Compiled);
        private static readonly Regex MultiNewlineRegex = new Regex(@"\n{3,}", RegexOptions.Compiled);

        /// <summary>
        /// Chia văn bản thành các chunk theo ranh giới ngữ nghĩa.
        /// </summary>
        /// <param name="text">Nội dung văn bản đầu vào</param>
        /// <param name="maxWordsPerChunk">
        ///   Số từ tối đa mỗi chunk (mặc định 400).
        ///   Có thể vượt nếu một câu đơn đã dài hơn giới hạn.
        /// </param>
        /// <param name="overlapSentences">
        ///   Số câu cuối chunk trước được thêm vào đầu chunk mới làm ngữ cảnh (mặc định 2).
        /// </param>
        public static List<string> SplitText(string text,
                                             int maxWordsPerChunk  = 400,
                                             int overlapSentences  = 2)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            // 1. Chuẩn hoá
            text = Normalize(text);

            // 2. Trích xuất câu
            var sentences = ExtractSentences(text);
            if (sentences.Count == 0)
                return new List<string>();

            // 3. Gom câu thành chunk
            return BuildChunks(sentences, maxWordsPerChunk, overlapSentences);
        }

        // ──────────────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ──────────────────────────────────────────────────────────────────────

        private static string Normalize(string text)
        {
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");
            text = MultiNewlineRegex.Replace(text, "\n\n"); // tối đa 2 dòng trống
            text = MultiSpaceRegex.Replace(text, " ");      // thu khoảng trắng trong dòng
            return text.Trim();
        }

        /// <summary>
        /// Tách văn bản thành danh sách câu, ưu tiên ranh giới đoạn văn.
        /// </summary>
        private static List<string> ExtractSentences(string text)
        {
            var result = new List<string>();

            // Đoạn văn được phân tách bằng 2 dòng trống liên tiếp
            var paragraphs = text.Split(new[] { "\n\n" },
                                        StringSplitOptions.RemoveEmptyEntries);

            foreach (var para in paragraphs)
            {
                var trimmed = para.Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) continue;

                var paraSentences = SplitParagraphToSentences(trimmed);
                result.AddRange(paraSentences.Where(s => !string.IsNullOrWhiteSpace(s)));
            }

            return result;
        }

        private static List<string> SplitParagraphToSentences(string paragraph)
        {
            // Thay thế dòng đơn (không phải đoạn) bằng khoảng trắng để gom lại
            var flat = paragraph.Replace("\n", " ").Trim();

            // Tách theo regex kết thúc câu
            var parts = SentenceEndRegex.Split(flat);
            var sentences = new List<string>();

            foreach (var part in parts)
            {
                var s = part.Trim();
                if (string.IsNullOrWhiteSpace(s)) continue;

                // Câu đơn cực dài (>600 từ): tách thêm tại mệnh đề ; ,
                if (WordCount(s) > 600)
                    sentences.AddRange(SplitLongSentence(s));
                else
                    sentences.Add(s);
            }

            return sentences;
        }

        /// <summary>
        /// Câu quá dài: tách tại dấu chấm phẩy hoặc dấu phẩy,
        /// gom cho đến khi đạt ~150 từ mới ngắt.
        /// </summary>
        private static IEnumerable<string> SplitLongSentence(string sentence)
        {
            var clauses = ClauseEndRegex.Split(sentence);
            var sb      = new StringBuilder();
            var result  = new List<string>();

            foreach (var clause in clauses)
            {
                if (sb.Length > 0) sb.Append(' ');
                sb.Append(clause.Trim());

                if (WordCount(sb.ToString()) >= 150)
                {
                    result.Add(sb.ToString().Trim());
                    sb.Clear();
                }
            }

            if (sb.Length > 0)
                result.Add(sb.ToString().Trim());

            return result;
        }

        /// <summary>
        /// Thuật toán gom câu thành chunk:
        ///   - Thêm câu vào chunk hiện tại đến khi gần đạt maxWords
        ///   - Khi chunk đầy: lưu lại, rồi bắt đầu chunk mới với N câu cuối làm overlap
        /// </summary>
        private static List<string> BuildChunks(List<string> sentences,
                                                int maxWords,
                                                int overlapSentences)
        {
            var chunks          = new List<string>();
            var window          = new List<string>(); // câu trong chunk đang xây dựng
            int windowWordCount = 0;

            for (int i = 0; i < sentences.Count; i++)
            {
                var sentence   = sentences[i];
                int sentWords  = WordCount(sentence);

                bool willExceed = windowWordCount + sentWords > maxWords;
                bool windowFull = willExceed && window.Count > 0;

                if (windowFull)
                {
                    // Lưu chunk hiện tại
                    chunks.Add(string.Join(" ", window));

                    // Giữ lại N câu cuối làm overlap cho chunk tiếp theo
                    int overlapStart  = Math.Max(0, window.Count - overlapSentences);
                    var overlapWindow = window.Skip(overlapStart).ToList();

                    window          = overlapWindow;
                    windowWordCount = window.Sum(WordCount);
                }

                window.Add(sentence);
                windowWordCount += sentWords;
            }

            // Chunk cuối
            if (window.Count > 0)
                chunks.Add(string.Join(" ", window));

            return chunks;
        }

        private static int WordCount(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            return text.Split(new[] { ' ', '\t', '\n', '\r' },
                              StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }
}
