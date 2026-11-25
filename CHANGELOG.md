# 변경 기록 — Meta Search

버전 규칙: 작은 수정 `x.x.1` · 새 기능 `x.1.x` · 구조 변경 `1.x.x`

## 2.0.0 — 2026-11-25

ASP.NET WebForms 에서 ASP.NET Core 8 로 새로 만든 버전. 예전 버전은 `legacy/` 폴더로 옮겼습니다.

- ASP.NET Core 8 Razor Pages + PostgreSQL(Dapper), 라즈베리파이 4 + Cloudflare Tunnel 운영
- 구글·네이버·다음·Brave 결과 RRF 통합 순위, 인기 검색어 TOP10·실시간 트렌드, AI 추천 검색어
- domang 통합 계정(로그인·가입·찾기 한 창), 통합 관리자 페이지
- 검색 노출 정리: 사이트맵 자동 생성, 설명문 수정, 옛 주소 301 이동
- 보안: 인증코드 대입 차단, 평가 실행 관리자 전용, 댓글·좋아요 요청 보호, 서버는 라즈베리파이 안에서만 열림
