-- MetaSearch DB 마이그레이션 스크립트
-- 실행: psql -U [user] -d [dbname] -f migration.sql

-- B1: Brave 엔진 개인화 점수 컬럼 추가
ALTER TABLE members ADD COLUMN IF NOT EXISTS bravescore DOUBLE PRECISION DEFAULT 0;

-- C1: API 호출 수 카운팅 테이블
CREATE TABLE IF NOT EXISTS api_usage (
    engine VARCHAR(20) NOT NULL,
    call_date DATE NOT NULL,
    call_count INT NOT NULL DEFAULT 0,
    PRIMARY KEY (engine, call_date)
);
