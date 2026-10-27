-- ============================================================
-- Supabase setup cho Battleship (chạy trong SQL Editor của Supabase)
--
-- THỨ TỰ: (1) chạy Server 1 lần với BATTLESHIP_DB=supabase để EF Core
--             tự tạo các bảng "Users", "Matches", "MatchDetails";
--         (2) rồi mới chạy file này.
--
-- MỤC ĐÍCH: Supabase tự mở REST API cho mọi bảng trong schema public.
-- Mặc định khóa "anon" (công khai) có thể ĐỌC và GHI các bảng đó nếu chưa bật RLS,
-- tức là ai có anon key đều xem được cột Password (hash BCrypt) của mọi người.
-- Script này khóa bảng gốc lại và chỉ mở 1 view an toàn cho bảng xếp hạng.
-- ============================================================

-- 1) Bật Row Level Security. Không có policy nào => anon/authenticated bị từ chối.
ALTER TABLE "Users"        ENABLE ROW LEVEL SECURITY;
ALTER TABLE "Matches"      ENABLE ROW LEVEL SECURITY;
ALTER TABLE "MatchDetails" ENABLE ROW LEVEL SECURITY;

-- 2) Thu hồi quyền trực tiếp trên bảng gốc (phòng thủ thêm 1 lớp).
REVOKE ALL ON "Users"        FROM anon, authenticated;
REVOKE ALL ON "Matches"      FROM anon, authenticated;
REVOKE ALL ON "MatchDetails" FROM anon, authenticated;

-- 3) View công khai: chỉ gồm cột an toàn, KHÔNG có Password.
--    View chạy bằng quyền chủ sở hữu (postgres) nên vẫn đọc được bảng gốc
--    dù bảng gốc đã khóa RLS.
CREATE OR REPLACE VIEW public.leaderboard AS
SELECT
    "Username",
    "Wins",
    "Losses",
    "Score"
FROM "Users";

-- 4) Chỉ cho phép ĐỌC view này.
GRANT SELECT ON public.leaderboard TO anon, authenticated;

-- 5) Kiểm tra nhanh (chạy riêng):
--    SELECT * FROM public.leaderboard ORDER BY "Score" DESC LIMIT 10;

-- ============================================================
-- DỮ LIỆU MẪU cho buổi demo (tùy chọn). Hash dưới đây là placeholder,
-- user seed này KHÔNG đăng nhập được; chỉ để bảng xếp hạng có dữ liệu.
-- Muốn đăng nhập thì đăng ký bằng game để BCrypt tạo hash thật.
-- ============================================================
-- Lưu ý: cột "Id" (text) và "Condition" (boolean) là NOT NULL và KHÔNG có giá trị mặc định
-- ở phía DB (Id do code C# tự sinh Guid), nên khi INSERT tay phải điền cả hai.
-- INSERT INTO "Users" ("Id","Condition","Username","Password","Wins","Losses","Score") VALUES
--   ('seed-0001', false, 'admiral_a', 'seed-not-a-real-hash', 12, 3, 1200),
--   ('seed-0002', false, 'captain_b', 'seed-not-a-real-hash',  9, 5,  900),
--   ('seed-0003', false, 'sailor_c',  'seed-not-a-real-hash',  4, 8,  400);
-- Dọn dữ liệu mẫu sau demo:  DELETE FROM "Users" WHERE "Id" LIKE 'seed-%';
