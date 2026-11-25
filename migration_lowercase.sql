-- 기존 회원 아이디를 모두 소문자로 변환
UPDATE members SET userid = LOWER(userid) WHERE userid != LOWER(userid);
