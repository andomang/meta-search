-- ═══ 포트폴리오 기능 추가 마이그레이션 ═══
-- 실행: psql -U metauser -d meta -f migrate_portfolio.sql

-- 1. 북마크 테이블 (검색 결과 저장)
CREATE TABLE IF NOT EXISTS bookmarks (
    id SERIAL PRIMARY KEY,
    userid VARCHAR(50) NOT NULL,
    title TEXT NOT NULL DEFAULT '',
    url TEXT NOT NULL,
    description TEXT DEFAULT '',
    source VARCHAR(20) DEFAULT '',
    folder VARCHAR(50) DEFAULT '기본',
    created_at TIMESTAMP DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS idx_bookmarks_userid ON bookmarks(userid);

-- 2. bbs 테이블에 uploadtime 기본값이 없으면 추가 (사이트맵용)
-- (기존 테이블이므로 ALTER만)
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name='bbs' AND column_name='regdate'
    ) THEN
        ALTER TABLE bbs ADD COLUMN regdate TIMESTAMP DEFAULT NOW();
        UPDATE bbs SET regdate = uploadtime WHERE regdate IS NULL;
    END IF;
END $$;
