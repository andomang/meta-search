# Meta Search

구글·네이버·다음·Brave 검색 결과를 한 번에 모아 순위를 다시 매기는(RRF) 통합 검색엔진입니다.

**[검색하기](https://domang.org/metasearch)** · [소개 페이지](https://domang.org/metaindex) · [성능 평가 리포트](https://domang.org/metasearch/report)

## 구성

- ASP.NET Core 8 Razor Pages + PostgreSQL(Dapper), 라즈베리파이 4 + Cloudflare Tunnel
- 검색: Google(Serper)·Naver·Daum(Kakao)·Brave API → RRF 통합 순위, 인기 검색어·실시간 트렌드, Claude API 추천 검색어
- 커뮤니티 게시판, 북마크, 대시보드, 설정, 관리자 페이지
- domang.org 의 포트폴리오·프로젝트 소개 페이지와 통합 계정도 이 서버에서 함께 돌아갑니다

## 폴더

| 폴더 | 내용 |
|---|---|
| 루트 | 지금 운영 중인 ASP.NET Core 버전 (`Program.cs`, `Pages/`, `Services/`, `wwwroot/`) |
| `legacy/` | 예전 ASP.NET WebForms 버전 (릴리스 1.0.3 까지) |

Cloud Drive 코드는 [andomang/drive](https://github.com/andomang/drive), 게임은 [andomang/pokemon_astera](https://github.com/andomang/pokemon_astera) 에 따로 있고, 전체는 [andomang/domang](https://github.com/andomang/domang) 에 있습니다. 비밀 설정은 `appsettings.example.json` 을 복사해 `appsettings.json` 으로 만들어 채웁니다.
