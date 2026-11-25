# domang.org

개발자 안도영의 프로젝트 모음입니다. 라즈베리파이 한 대에서 운영하며, 모든 프로젝트는 [domang.org](https://domang.org)에서 바로 써 볼 수 있습니다.

| 프로젝트 | 설명 | 바로가기 | 코드 |
|---|---|---|---|
| **Meta Search** | 구글·네이버·다음·Brave 검색 결과를 RRF로 합친 통합 검색엔진 | [소개](https://domang.org/metaindex) · [사용](https://domang.org/metasearch) | [`rasp/MetaSearch`](rasp/MetaSearch) |
| **포켓몬스터 아스테라** | HTML5 Canvas 브라우저 팬게임 (오리지널 스토리) | [소개](https://domang.org/pokeindex) · [플레이](https://domang.org/pokemon) | [`pokemon`](pokemon) |
| **Cloud Drive** | 개인 클라우드 드라이브 | [소개](https://domang.org/driveindex) | [`rasp/MetaSearch`](rasp/MetaSearch) (같은 서버) |
| **OTT Multi-View** | 여러 OTT를 한 화면에서 보는 데스크톱 앱 | [소개](https://domang.org/ottindex) | [andomang/OTT](https://github.com/andomang/OTT) |

## 폴더 구조

```
rasp/
  MetaSearch/        ASP.NET Core 8 서버 (domang.org 전체: 포트폴리오, 검색, 드라이브, 게임 API)
  deploy.bat         빌드 → 게임 묶기 → 라즈베리파이로 바뀐 파일만 업로드
  bundle-pokemon.mjs 게임 스크립트를 하나로 묶고 미리 압축
pokemon/             포켓몬스터 아스테라 (index.html + game/)
```

## 운영 환경

- 서버: Raspberry Pi (linux-arm64), systemd 서비스, PostgreSQL
- 앞단: Cloudflare (HTTPS, 캐시)
- 배포: Windows에서 `rasp\deploy.bat` 실행 → 해시 비교로 바뀐 파일만 전송 후 서비스 재시작

## 저장소에 없는 것

- `rasp/MetaSearch/appsettings.json`: API 키·DB 비밀번호. [`appsettings.example.json`](rasp/MetaSearch/appsettings.example.json)을 복사해 채우세요.
- 게임 그래픽·사운드 에셋: 저작권 때문에 넣지 않았습니다. 자세한 내용은 [`pokemon/README.md`](pokemon/README.md)를 보세요.

## 연락처

[metasearch@domang.org](mailto:metasearch@domang.org) · [github.com/andomang](https://github.com/andomang)
