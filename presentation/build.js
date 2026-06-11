// 전체 덱 빌드. 파트 단위로 슬라이드 함수를 추가한다(현재 Part 1: 1~3).
const pptxgen = require("pptxgenjs");
const T = require("./theme");
const { DECK, COLOR, FONT, RIGHT_CARD, SHOT_RATIO } = T;

const SHOT = {
  normal: "../스크린샷 2026-06-11 091749.png", // MAIN 정상
  danger: "../스크린샷 2026-06-11 093028.png", // 자동정지 DANGER
};
const TOTAL = 22;
// 빌드 산출물명은 OUT 환경변수로 덮어쓸 수 있음(PowerPoint가 deck.pptx를 잠갔을 때 미리보기용).

const pres = new pptxgen();
pres.defineLayout({ name: "WIDE", width: DECK.W, height: DECK.H });
pres.layout = "WIDE";
pres.author = "김길동, 박재민, 최현규";
pres.title = "반도체 장비 센서 제어 시스템";

const newSlide = () => {
  const s = pres.addSlide();
  s.background = { color: COLOR.BG };
  T.sideBar(pres, s);
  return s;
};

// ── 슬라이드 1: 표지 ────────────────────────────────────────
function s01_cover() {
  const s = newSlide();
  const LX = 0.75;
  s.addText("교육과정 최종 프로젝트 · 발표", {
    x: LX, y: 1.2, w: 6, h: 0.3, margin: 0,
    fontFace: FONT.SEMI, fontSize: 12, color: COLOR.PRIMARY, charSpacing: 2, valign: "middle",
  });
  s.addText([
    { text: "반도체 장비", options: { breakLine: true } },
    { text: "센서 제어 시스템" },
  ], {
    x: LX - 0.02, y: 1.6, w: 5.9, h: 1.9, margin: 0,
    fontFace: FONT.BLACK, fontSize: 46, color: COLOR.INK, align: "left", valign: "top", lineSpacingMultiple: 1.04,
  });
  s.addText([
    { text: "TwinCAT ADS 실시간 센서 감시 · AI 위험 규칙,", options: { breakLine: true } },
    { text: "자동 대응 · 이력 관리 · 접근 통제" },
  ], {
    x: LX, y: 3.65, w: 5.5, h: 0.9, margin: 0,
    fontFace: FONT.MED, fontSize: 15.5, color: COLOR.SUB, align: "left", valign: "top", lineSpacingMultiple: 1.18,
  });

  // 기술 칩
  const chips = [["WPF · .NET", 1.55], ["TwinCAT ADS", 1.75], ["SQLite", 1.05], ["Flask", 0.95]];
  let cx = LX;
  chips.forEach(([text, w]) => { T.chip(pres, s, { x: cx, y: 4.62, w, text }); cx += w + 0.18; });

  // 메타 (placeholder)
  s.addShape(pres.shapes.LINE, { x: LX, y: 5.55, w: 5.7, h: 0, line: { color: COLOR.BORDER, width: 1 } });
  const meta = [["팀", "김길동, 박재민, 최현규", LX], ["발표자", "김길동", LX + 2.55], ["발표 일자", "2026. 06. 18", LX + 4.55]];
  meta.forEach(([label, value, x]) => {
    s.addText(label, { x, y: 5.7, w: 2.4, h: 0.25, margin: 0, fontFace: FONT.MED, fontSize: 10, color: COLOR.FAINT, charSpacing: 1, valign: "middle" });
    s.addText(value, { x, y: 5.95, w: 2.5, h: 0.3, margin: 0, fontFace: FONT.SEMI, fontSize: 13, color: COLOR.INK, valign: "middle" });
  });

  // 우측 히어로 스크린샷
  const cardX = 6.85, cardY = 1.3, cardW = 5.85;
  const ch = T.screenshotCard(pres, s, { path: SHOT.normal, x: cardX, y: cardY, w: cardW, imgRatio: SHOT_RATIO });
  T.captionBar(pres, s, {
    x: cardX, y: cardY + ch + 0.18, w: cardW, accent: COLOR.GREEN,
    runs: [
      { text: "정상 운전 화면", options: { fontFace: FONT.SEMI, color: COLOR.INK } },
      { text: ",  모든 센서 정상 · AI 감시 중", options: { fontFace: FONT.MED, color: COLOR.SUB } },
    ],
  });
}

// ── 슬라이드 2: 목차 ────────────────────────────────────────
function s02_agenda() {
  const s = newSlide();
  T.header(pres, s, { kicker: "Agenda", title: "목차" });
  const items = [
    ["01", "프로젝트 개요", "무엇을·왜, 한눈에 보기"],
    ["02", "문제 정의 & 배경", "필요성과 요구사항 도출"],
    ["03", "설계 의사결정", "아키텍처 · 기술 선택 근거"],
    ["04", "핵심 기능 구현", "센서 · 위험 · 제어 · 인증 · 데이터"],
    ["05", "검증 & 데모", "실제 실행 화면 워크스루"],
    ["06", "회고 & 향후", "트러블슈팅 · 개선 방향"],
  ];
  const cols = [0.75, 6.95];
  const startY = 2.2, rowH = 1.45;
  items.forEach(([num, title, sub], i) => {
    const x = cols[i % 2];
    const y = startY + Math.floor(i / 2) * rowH;
    s.addText(num, { x, y: y - 0.06, w: 0.9, h: 0.7, margin: 0, fontFace: FONT.XBOLD, fontSize: 30, color: COLOR.PRIMARY, valign: "middle" });
    s.addText(title, { x: x + 0.95, y: y - 0.02, w: 4.6, h: 0.4, margin: 0, fontFace: FONT.SEMI, fontSize: 16, color: COLOR.INK, valign: "middle" });
    s.addText(sub, { x: x + 0.95, y: y + 0.4, w: 4.7, h: 0.35, margin: 0, fontFace: FONT.REG, fontSize: 12, color: COLOR.SUB, valign: "middle" });
  });
  T.footer(pres, s, { page: 2, total: TOTAL });
}

// ── 슬라이드 3: 프로젝트 한눈에 ─────────────────────────────
function s03_overview() {
  const s = newSlide();
  T.header(pres, s, { kicker: "Part 1 · 도입", title: "프로젝트 한눈에 보기" });
  s.addText([
    { text: "실 장비 없이 반도체 장비 센서 감시·제어를 학습하는,", options: { breakLine: true } },
    { text: "Windows WPF 데스크톱 트레이너입니다." },
  ], {
    x: 0.75, y: 1.55, w: 5.6, h: 0.6, margin: 0,
    fontFace: FONT.MED, fontSize: 14, color: COLOR.SUB, align: "left", valign: "top", lineSpacingMultiple: 1.1,
  });
  T.numberedRows(pres, s, {
    x: 0.75, startY: 2.35, items: [
      ["실시간 센서 감시", "압력·진동·온도·습도를 약 1Hz로 폴링·표시"],
      ["하드웨어 연동", "TwinCAT ADS로 아날로그/디지털 I/O·램프 제어"],
      ["위험 자동 대응", "임계값 평가 → 누적 → 자동 종료 + 경고 램프"],
      ["데이터 · 연동", "SQLite 영속화 + Flask 인증·텔레메트리"],
    ],
  });
  T.screenshotCard(pres, s, { path: SHOT.normal, x: RIGHT_CARD.x, y: RIGHT_CARD.y, w: RIGHT_CARD.w, imgRatio: SHOT_RATIO, inset: RIGHT_CARD.inset });
  T.captionBar(pres, s, {
    x: RIGHT_CARD.x, y: 5.598, w: RIGHT_CARD.w, accent: COLOR.GREEN,
    runs: [
      { text: "정상 운전", options: { fontFace: FONT.SEMI, color: COLOR.INK } },
      { text: ",  모든 센서 정상 · AI 감시 중", options: { fontFace: FONT.MED, color: COLOR.SUB } },
    ],
  });
  T.footer(pres, s, { page: 3, total: TOTAL });
}

// ── 슬라이드 4: 배경 ────────────────────────────────────────
function s04_background() {
  const s = newSlide();
  T.header(pres, s, { kicker: "Part 2 · 문제 정의 & 배경", title: "왜 센서 트레이너가 필요한가" });
  s.addText([
    { text: "반도체 장비는 압력·진동·온도·습도 등 여러 센서로 상태를 감시하지만,", options: { breakLine: true } },
    { text: "교육 현장에서 이를 실장비로 직접 다뤄 보기에는 현실적인 제약이 많습니다." },
  ], {
    x: 0.75, y: 1.55, w: 11.9, h: 0.7, margin: 0,
    fontFace: FONT.MED, fontSize: 14.5, color: COLOR.SUB, align: "left", valign: "top", lineSpacingMultiple: 1.2,
  });

  const cardW = 3.7, gap = 0.405, cardY = 2.55, cardH = 2.25;
  const cards = [
    ["제약 1", "고가 · 제한된 접근", "실제 장비는 비싸고, 교육용으로 상시 운용하며 학습자가 자유롭게 조작하기 어렵습니다."],
    ["제약 2", "위험 상황 재현 곤란", "임계 초과나 자동 정지 같은 이상·위험 상황을 교육을 위해 일부러 만들어 보기 어렵습니다."],
    ["제약 3", "반복 실습의 한계", "안전과 비용 문제로 시행착오를 반복하는 실습이 제한되어 체득이 더딥니다."],
  ];
  cards.forEach(([kicker, title, desc], i) => {
    T.accentCard(pres, s, {
      x: 0.75 + i * (cardW + gap), y: cardY, w: cardW, h: cardH,
      accent: COLOR.AMBER, kicker, title, desc,
    });
  });

  // 해법 밴드
  const bandY = 5.35;
  s.addShape(pres.shapes.RECTANGLE, { x: 0.75, y: bandY, w: 11.95, h: 1.0, fill: { color: COLOR.PANEL }, line: { type: "none" } });
  s.addShape(pres.shapes.RECTANGLE, { x: 0.75, y: bandY, w: 0.08, h: 1.0, fill: { color: COLOR.GREEN }, line: { type: "none" } });
  s.addText([
    { text: "해결  ", options: { fontFace: FONT.XBOLD, color: COLOR.GREEN, fontSize: 13 } },
    { text: "실장비 없이 센서 감시와 위험 대응을 ", options: { fontFace: FONT.SEMI, color: COLOR.INK } },
    { text: "반복 학습", options: { fontFace: FONT.XBOLD, color: COLOR.INK } },
    { text: "할 수 있는 Windows 데스크톱 트레이너를 만든다.", options: { fontFace: FONT.SEMI, color: COLOR.INK } },
  ], {
    x: 1.12, y: bandY, w: 11.4, h: 1.0, margin: 0, fontSize: 16, align: "left", valign: "middle",
  });

  T.footer(pres, s, { page: 4, total: TOTAL });
}

// ── 슬라이드 5: 문제 정의 & 목표 ────────────────────────────
function s05_goals() {
  const s = newSlide();
  T.header(pres, s, { kicker: "Part 2 · 문제 정의 & 배경", title: "문제 정의와 4대 목표" });
  s.addText("트레이너가 반드시 해결해야 할 과제를 네 가지 목표로 정의했습니다.", {
    x: 0.75, y: 1.55, w: 11.9, h: 0.4, margin: 0,
    fontFace: FONT.MED, fontSize: 14.5, color: COLOR.SUB, align: "left", valign: "middle",
  });

  const goals = [
    ["목표 01", "실시간 센서 감시", "압력·진동·온도·습도를 약 1Hz로 폴링해 현재 값과 상태를 즉시 보여 준다.", COLOR.PRIMARY],
    ["목표 02", "위험 자동 대응", "임계값을 평가해 위험을 누적하고, 한도 도달 시 공정을 자동으로 종료한다.", COLOR.RED],
    ["목표 03", "이력 관리", "센서 스냅샷과 활동 로그를 영속화하고, 화면에서 조회·추적할 수 있게 한다.", COLOR.GREEN],
    ["목표 04", "접근 통제", "인증과 권한(Operator·Admin)으로 민감한 조작과 설정을 보호한다.", COLOR.AMBER],
  ];
  const cardW = 5.77, gapX = 0.41, gapY = 0.34, cardH = 1.85, startY = 2.35;
  goals.forEach(([kicker, title, desc, accent], i) => {
    const x = 0.75 + (i % 2) * (cardW + gapX);
    const y = startY + Math.floor(i / 2) * (cardH + gapY);
    T.accentCard(pres, s, { x, y, w: cardW, h: cardH, accent, kicker, title, desc });
  });

  T.footer(pres, s, { page: 5, total: TOTAL });
}

// ── 슬라이드 6: 요구사항 ────────────────────────────────────
function s06_requirements() {
  const s = newSlide();
  T.header(pres, s, { kicker: "Part 2 · 문제 정의 & 배경", title: "요구사항 정의" });
  s.addText("목표를 기능 요구와 비기능 요구로 구체화했습니다.", {
    x: 0.75, y: 1.55, w: 11.9, h: 0.4, margin: 0,
    fontFace: FONT.MED, fontSize: 14.5, color: COLOR.SUB, align: "left", valign: "middle",
  });

  const panelW = 5.77, gapX = 0.41, panelY = 2.25, panelH = 4.35, panelX2 = 0.75 + panelW + gapX;
  const panel = (x, accent, label, rows) => {
    s.addShape(pres.shapes.RECTANGLE, { x, y: panelY, w: panelW, h: panelH, fill: { color: COLOR.BG }, line: { color: COLOR.BORDER, width: 1 }, shadow: T.shadow({ blur: 6, offset: 2, opacity: 0.08 }) });
    s.addShape(pres.shapes.RECTANGLE, { x, y: panelY, w: panelW, h: 0.62, fill: { color: accent }, line: { type: "none" } });
    s.addText(label, { x: x + 0.34, y: panelY, w: panelW - 0.5, h: 0.62, margin: 0, fontFace: FONT.BOLD, fontSize: 15, color: "FFFFFF", valign: "middle" });
    const rowY = panelY + 0.95, rowH = (panelH - 1.15) / rows.length;
    rows.forEach(([term, desc], i) => {
      T.checkRow(pres, s, { x: x + 0.34, y: rowY + i * rowH, w: panelW - 0.6, accent, term, desc });
    });
  };

  panel(0.75, COLOR.PRIMARY, "기능 요구사항", [
    ["센서 실시간 감시", "값·상태 표시"],
    ["위험 평가 · 자동 종료", "임계 초과 누적 대응"],
    ["램프 · 공정 제어", "가동/경고/AI 램프"],
    ["사용자 인증 · 권한", "Operator · Admin"],
    ["활동 · 센서 로그", "기록 · 조회"],
  ]);
  panel(panelX2, COLOR.GREEN, "비기능 요구사항", [
    ["실시간성", "약 1Hz 폴링 · 즉시 반영"],
    ["UI 비차단", "네트워크 · 폴링이 화면을 막지 않음"],
    ["안정성", "예외 격리 · 통신 끊김 허용"],
    ["디자이너 미리보기", "하드웨어 없이 스텁으로 렌더"],
    ["확장성", "인터페이스 우선 설계"],
  ]);

  T.footer(pres, s, { page: 6, total: TOTAL });
}

// ── 슬라이드 7: 기술 스택 & 선택 근거 ───────────────────────
function s07_stack() {
  const s = newSlide();
  T.header(pres, s, { kicker: "Part 3 · 설계 의사결정", title: "기술 스택 & 선택 근거" });
  s.addText("사용자에 가까운 표현 계층부터 외부 연동까지, 계층별로 기술을 선택한 이유입니다.", {
    x: 0.75, y: 1.5, w: 11.9, h: 0.4, margin: 0,
    fontFace: FONT.MED, fontSize: 14, color: COLOR.SUB, valign: "middle",
  });

  const BX = 0.75, BW = 11.95, BY = 2.05, BH = 1.0, GAP = 0.16;
  const DIV = BX + 5.0;
  const bands = [
    ["표현 · UI", "WPF · .NET Framework 4.7.2", "데스크톱 네이티브 UI와 강력한 데이터바인딩으로 실시간 상태 표시에 적합", null],
    ["패턴", "MVVM · 프레임워크 없이 직접 구현", "ViewModelBase·RelayCommand로 의존성 흐름을 명시적으로 관리, 외부 종속 최소화", null],
    ["서비스", "인터페이스 우선 서비스 계층", "ITrainerClient 등 추상화로 하드웨어 없이 스텁·디자이너 미리보기·교체가 가능", null],
    ["외부 연동", null, "트레이너 통신 · 로컬 영속화 · 인증/텔레메트리를 가벼운 표준 기술로 분리", ["TwinCAT ADS", "SQLite", "Flask"]],
  ];
  bands.forEach(([role, tech, reason, chips], i) => {
    const y = BY + i * (BH + GAP);
    const accent = i === 3 ? COLOR.PRIMARY_DK : COLOR.PRIMARY;
    s.addShape(pres.shapes.RECTANGLE, { x: BX, y, w: BW, h: BH, fill: { color: COLOR.PANEL }, line: { type: "none" }, shadow: T.shadow({ blur: 5, offset: 2, opacity: 0.07 }) });
    s.addShape(pres.shapes.RECTANGLE, { x: BX, y, w: 0.08, h: BH, fill: { color: accent }, line: { type: "none" } });
    s.addText(role.toUpperCase(), {
      x: BX + 0.34, y: y + 0.19, w: 4.4, h: 0.24, margin: 0,
      fontFace: FONT.SEMI, fontSize: 10.5, color: accent, charSpacing: 1.5, valign: "middle",
    });
    if (chips) {
      let cx = BX + 0.34;
      const cw = [1.6, 1.05, 0.95];
      chips.forEach((t, k) => { T.chip(pres, s, { x: cx, y: y + 0.46, w: cw[k], text: t, fill: COLOR.BG, border: COLOR.BORDER }); cx += cw[k] + 0.14; });
    } else {
      s.addText(tech, {
        x: BX + 0.34, y: y + 0.45, w: 4.45, h: 0.38, margin: 0,
        fontFace: FONT.BOLD, fontSize: 14, color: COLOR.INK, valign: "middle",
      });
    }
    s.addShape(pres.shapes.LINE, { x: DIV, y: y + 0.2, w: 0, h: BH - 0.4, line: { color: COLOR.BORDER, width: 1 } });
    s.addText(reason, {
      x: DIV + 0.32, y, w: BX + BW - DIV - 0.55, h: BH, margin: 0,
      fontFace: FONT.MED, fontSize: 13, color: COLOR.SUB, align: "left", valign: "middle", lineSpacingMultiple: 1.12,
    });
  });

  T.footer(pres, s, { page: 7, total: TOTAL });
}

// ── 슬라이드 8: 시스템 아키텍처 (다이어그램) ────────────────
function s08_architecture() {
  const s = newSlide();
  T.header(pres, s, { kicker: "Part 3 · 설계 의사결정", title: "시스템 아키텍처" });
  s.addText("MVVM 4계층으로 책임을 분리하고, 합성 루트(MainViewModel)가 전체 객체 그래프를 직접 구성합니다.", {
    x: 0.75, y: 1.5, w: 11.9, h: 0.4, margin: 0,
    fontFace: FONT.MED, fontSize: 14, color: COLOR.SUB, valign: "middle",
  });

  const SLATE = "586273";
  const COL_W = 2.6, GAP = 0.5, CY = 2.15, CH = 3.45, HEAD_H = 0.52;
  const cols = [
    { x: 0.75, head: "Views · XAML", accent: COLOR.PRIMARY, items: [["ConsoleView"], ["AuthView"], ["LogView"], ["SettingsView"]] },
    { x: 3.85, head: "ViewModels", accent: COLOR.PRIMARY, items: [["ConsoleViewModel", "폴링·위험·램프", true], ["AuthViewModel"], ["LogViewModel"], ["SettingsViewModel"]] },
    { x: 6.95, head: "Services · 인터페이스", accent: COLOR.PRIMARY, items: [["ITrainerClient"], ["IAuthService"], ["IRemoteTelemetryService"], ["Store · Repository"]] },
    { x: 10.05, head: "외부 시스템", accent: SLATE, items: [["TwinCAT ADS", "장비 I/O · 포트 851"], ["SQLite", "로컬 DB · WAL"], ["Flask", "인증 · 텔레메트리"]] },
  ];

  cols.forEach((col) => {
    // 컬럼 컨테이너
    s.addShape(pres.shapes.RECTANGLE, { x: col.x, y: CY, w: COL_W, h: CH, fill: { color: "FBFCFD" }, line: { color: COLOR.BORDER, width: 1 }, shadow: T.shadow({ blur: 6, offset: 2, opacity: 0.08 }) });
    // 헤더 밴드
    s.addShape(pres.shapes.RECTANGLE, { x: col.x, y: CY, w: COL_W, h: HEAD_H, fill: { color: col.accent }, line: { type: "none" } });
    s.addText(col.head, { x: col.x + 0.12, y: CY, w: COL_W - 0.24, h: HEAD_H, margin: 0, fontFace: FONT.BOLD, fontSize: 12, color: "FFFFFF", align: "center", valign: "middle" });
    // 아이템 박스
    const n = col.items.length;
    const innerTop = CY + HEAD_H + 0.16, innerBot = CY + CH - 0.16, ig = 0.14;
    const ih = (innerBot - innerTop - (n - 1) * ig) / n;
    col.items.forEach(([term, sub, star], j) => {
      const iy = innerTop + j * (ih + ig);
      const bordered = star ? col.accent : COLOR.BORDER;
      s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: col.x + 0.16, y: iy, w: COL_W - 0.32, h: ih, rectRadius: 0.05, fill: { color: COLOR.BG }, line: { color: bordered, width: star ? 1.5 : 1 } });
      if (sub) {
        s.addText(term, { x: col.x + 0.16, y: iy, w: COL_W - 0.32, h: ih * 0.52, margin: 0, fontFace: FONT.SEMI, fontSize: 11, color: star ? col.accent : COLOR.INK, align: "center", valign: "middle" });
        s.addText(sub, { x: col.x + 0.16, y: iy + ih * 0.5, w: COL_W - 0.32, h: ih * 0.5, margin: 0, fontFace: FONT.REG, fontSize: 8.5, color: COLOR.SUB, align: "center", valign: "middle" });
      } else {
        s.addText(term, { x: col.x + 0.16, y: iy, w: COL_W - 0.32, h: ih, margin: 0, fontFace: FONT.SEMI, fontSize: 10.5, color: COLOR.INK, align: "center", valign: "middle" });
      }
    });
  });

  // 컬럼 사이 화살표 + 라벨
  const cyMid = CY + CH / 2;
  const arrows = [
    { label: "바인딩", both: true },
    { label: "호출", both: false },
    { label: "I/O", both: false },
  ];
  arrows.forEach((a, i) => {
    const x1 = cols[i].x + COL_W + 0.07, x2 = cols[i + 1].x - 0.07;
    s.addShape(pres.shapes.LINE, {
      x: x1, y: cyMid, w: x2 - x1, h: 0,
      line: { color: COLOR.PRIMARY_DK, width: 2, endArrowType: "triangle", beginArrowType: a.both ? "triangle" : "none" },
    });
    const mid = (x1 + x2) / 2;
    s.addText(a.label, { x: mid - 0.45, y: cyMid - 0.42, w: 0.9, h: 0.24, margin: 0, fontFace: FONT.SEMI, fontSize: 9, color: COLOR.PRIMARY_DK, align: "center", valign: "middle" });
  });

  // 하단: 합성 루트 밴드
  const bandY = 5.95, bandH = 0.82;
  // 밴드 → ViewModels/Services 로 향하는 점선 커넥터(구성·주입). 위(컬럼)로 화살표.
  const connTop = CY + CH, connH = bandY - connTop;
  [cols[1], cols[2]].forEach((c) => {
    s.addShape(pres.shapes.LINE, { x: c.x + COL_W / 2, y: connTop, w: 0, h: connH, line: { color: COLOR.FAINT, width: 1, dashType: "dash", beginArrowType: "triangle" } });
  });
  s.addShape(pres.shapes.RECTANGLE, { x: 0.75, y: bandY, w: 11.9, h: bandH, fill: { color: COLOR.PANEL }, line: { type: "none" } });
  s.addShape(pres.shapes.RECTANGLE, { x: 0.75, y: bandY, w: 0.08, h: bandH, fill: { color: COLOR.PRIMARY }, line: { type: "none" } });
  s.addText([
    { text: "합성 루트   ", options: { fontFace: FONT.XBOLD, color: COLOR.PRIMARY } },
    { text: "MainViewModel", options: { fontFace: FONT.BOLD, color: COLOR.INK } },
    { text: " 가  DatabaseService → Repository → Store → Service → 자식 VM 을 직접 구성·주입한다  (DI 컨테이너 없음)", options: { fontFace: FONT.MED, color: COLOR.SUB } },
  ], { x: 1.12, y: bandY, w: 11.4, h: bandH, margin: 0, fontSize: 12.5, align: "left", valign: "middle" });

  T.footer(pres, s, { page: 8, total: TOTAL });
}

// ── 슬라이드 9: 핵심 설계 결정 (결정·근거·효과 표) ──────────
function s09_decisions() {
  const s = newSlide();
  T.header(pres, s, { kicker: "Part 3 · 설계 의사결정", title: "핵심 설계 결정" });
  s.addText("세 가지 결정이 트레이너의 테스트 용이성·일관성을 만들었습니다.", {
    x: 0.75, y: 1.5, w: 11.9, h: 0.4, margin: 0,
    fontFace: FONT.MED, fontSize: 14, color: COLOR.SUB, valign: "middle",
  });

  const CX = 0.75, CW = 11.9;
  const TITLE_X = CX + 0.95, G_X = CX + 4.75, E_X = CX + 8.65;
  const DIV1 = CX + 4.45, DIV2 = CX + 8.35;
  const G_W = 3.4, E_W = 3.05;

  // 열 헤더
  [["설계 결정", TITLE_X], ["근거 · 왜", G_X], ["효과 · 결과", E_X]].forEach(([t, x]) => {
    s.addText(t.toUpperCase(), { x, y: 2.02, w: 3.4, h: 0.26, margin: 0, fontFace: FONT.SEMI, fontSize: 10.5, color: COLOR.FAINT, charSpacing: 1.5, valign: "middle" });
  });

  const decisions = [
    ["1", "인터페이스 우선 설계", "ITrainerClient · IAuthService 등으로 하드웨어 · 서버 · DB 의존을 추상화", "장비 없이 스텁 실행 · 테스트, XAML 디자이너 미리보기, 구현 교체가 쉬움", COLOR.PRIMARY],
    ["2", "Repository → Store 2계층", "원시 SQLite 접근(Repository)과 캐싱 · 변경통지 · 팬아웃(Store)을 분리", "VM은 Store만 의존, ActivityLogStore가 원격 텔레메트리까지 일관 전파", COLOR.GREEN],
    ["3", "단일 설정 소스", "모든 임계값 · 튜닝값을 AppSettingsStore의 Default* 상수로 한 곳에 정의", "위험 엔진과 설정 화면이 같은 소스를 공유해 값 불일치를 제거", COLOR.AMBER],
  ];
  const RY = 2.42, RH = 1.36, GAP = 0.15;
  decisions.forEach(([num, title, ground, effect, accent], i) => {
    const y = RY + i * (RH + GAP);
    s.addShape(pres.shapes.RECTANGLE, { x: CX, y, w: CW, h: RH, fill: { color: COLOR.PANEL }, line: { type: "none" }, shadow: T.shadow({ blur: 5, offset: 2, opacity: 0.07 }) });
    s.addShape(pres.shapes.RECTANGLE, { x: CX, y, w: 0.08, h: RH, fill: { color: accent }, line: { type: "none" } });
    // 번호 배지
    const bs = 0.5, by = y + (RH - bs) / 2;
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: CX + 0.32, y: by, w: bs, h: bs, rectRadius: 0.07, fill: { color: accent }, line: { type: "none" } });
    s.addText(num, { x: CX + 0.32, y: by, w: bs, h: bs, margin: 0, fontFace: FONT.XBOLD, fontSize: 18, color: "FFFFFF", align: "center", valign: "middle" });
    // 결정 제목
    s.addText(title, { x: TITLE_X, y, w: 3.05, h: RH, margin: 0, fontFace: FONT.BOLD, fontSize: 15, color: COLOR.INK, align: "left", valign: "middle" });
    // 구분선
    s.addShape(pres.shapes.LINE, { x: DIV1, y: y + 0.2, w: 0, h: RH - 0.4, line: { color: COLOR.BORDER, width: 1 } });
    s.addShape(pres.shapes.LINE, { x: DIV2, y: y + 0.2, w: 0, h: RH - 0.4, line: { color: COLOR.BORDER, width: 1 } });
    // 근거
    s.addText(ground, { x: G_X, y, w: G_W, h: RH, margin: 0, fontFace: FONT.MED, fontSize: 11.5, color: COLOR.SUB, align: "left", valign: "middle", lineSpacingMultiple: 1.15 });
    // 효과
    s.addText(effect, { x: E_X, y, w: E_W, h: RH, margin: 0, fontFace: FONT.MED, fontSize: 11.5, color: COLOR.INK, align: "left", valign: "middle", lineSpacingMultiple: 1.15 });
  });

  T.footer(pres, s, { page: 9, total: TOTAL });
}

// ── 슬라이드 10: 하드웨어 연동 (신호 흐름도) ────────────────
function s10_hardware() {
  const s = newSlide();
  T.header(pres, s, { kicker: "Part 4 · 핵심 기능 구현", title: "하드웨어 연동 · TwinCAT ADS" });
  s.addText("ADS 포트 851로 트레이너의 아날로그·디지털 입력을 읽고 램프 출력을 씁니다. 읽기·쓰기마다 짧게 연결합니다.", {
    x: 0.75, y: 1.5, w: 11.9, h: 0.4, margin: 0, fontFace: FONT.MED, fontSize: 13.5, color: COLOR.SUB, valign: "middle",
  });

  // 상단 흐름: WPF 앱 ⇄ ADS(851) ⇄ 트레이너
  const fy = 2.02, fh = 0.92, fyMid = fy + fh / 2;
  const flowBox = (x, w, fill, t1, t2, white) => {
    s.addShape(pres.shapes.RECTANGLE, { x, y: fy, w, h: fh, rectRadius: 0.06, fill: { color: fill }, line: fill === COLOR.BG ? { color: COLOR.BORDER, width: 1 } : { type: "none" }, shadow: T.shadow({ blur: 5, offset: 2, opacity: 0.08 }) });
    s.addText(t1, { x, y: fy + 0.15, w, h: 0.36, margin: 0, fontFace: FONT.BOLD, fontSize: 14, color: white ? "FFFFFF" : COLOR.INK, align: "center", valign: "middle" });
    s.addText(t2, { x, y: fy + 0.5, w, h: 0.3, margin: 0, fontFace: FONT.REG, fontSize: 10, color: white ? "DCE6F5" : COLOR.SUB, align: "center", valign: "middle" });
  };
  flowBox(0.75, 3.6, COLOR.BG, "WPF 앱", "AdsSensorTrainerClient", false);
  flowBox(5.45, 2.45, COLOR.PRIMARY, "TwinCAT ADS", "포트 851", true);
  flowBox(9.15, 3.5, COLOR.BG, "Beckhoff 트레이너", "NX 아날로그·디지털 I/O 모듈", false);
  [[4.35, 5.45], [7.9, 9.15]].forEach(([x1, x2]) => {
    s.addShape(pres.shapes.LINE, { x: x1, y: fyMid, w: x2 - x1, h: 0, line: { color: COLOR.PRIMARY_DK, width: 2, beginArrowType: "triangle", endArrowType: "triangle" } });
  });

  // 심볼 3종 카드
  const cy = 3.4, cw = 3.83, cgap = 0.2, ch = 1.85, chh = 0.46;
  const syms = [
    ["GVL.NX_AD4203", COLOR.PRIMARY, "아날로그 입력 · 16 byte", "읽기", "압력·진동·온도·습도 = Int16 4채널 (offset 0·2·4·6)"],
    ["GVL.NX_ID5342", COLOR.PRIMARY, "디지털 입력 · 비트필드", "읽기", "DI1~4 + 광·근접 센서(FOUP A·B) = 6비트"],
    ["GVL.NX_OD5121", COLOR.PRIMARY_DK, "디지털 출력 · 비트필드", "쓰기", "가동·공정·경고·AI 램프 4비트 (변경 불가 계약)"],
  ];
  syms.forEach(([sym, accent, type, dir, desc], i) => {
    const x = 0.75 + i * (cw + cgap);
    s.addShape(pres.shapes.RECTANGLE, { x, y: cy, w: cw, h: ch, fill: { color: COLOR.BG }, line: { color: COLOR.BORDER, width: 1 }, shadow: T.shadow({ blur: 6, offset: 2, opacity: 0.08 }) });
    s.addShape(pres.shapes.RECTANGLE, { x, y: cy, w: cw, h: chh, fill: { color: accent }, line: { type: "none" } });
    s.addText(sym, { x: x + 0.18, y: cy, w: cw - 0.36, h: chh, margin: 0, fontFace: FONT.BOLD, fontSize: 13, color: "FFFFFF", valign: "middle" });
    s.addText(type, { x: x + 0.22, y: cy + 0.58, w: cw - 1.3, h: 0.3, margin: 0, fontFace: FONT.SEMI, fontSize: 11, color: COLOR.INK, valign: "middle" });
    T.chip(pres, s, { x: x + cw - 1.0, y: cy + 0.56, w: 0.82, text: dir, fill: COLOR.PANEL });
    s.addText(desc, { x: x + 0.22, y: cy + 1.0, w: cw - 0.44, h: 0.55, margin: 0, fontFace: FONT.REG, fontSize: 10.5, color: COLOR.SUB, valign: "top", lineSpacingMultiple: 1.12 });
  });

  // 하단 노트 밴드
  const ny = 5.6, nh = 0.82;
  s.addShape(pres.shapes.RECTANGLE, { x: 0.75, y: ny, w: 11.9, h: nh, fill: { color: COLOR.PANEL }, line: { type: "none" } });
  s.addShape(pres.shapes.RECTANGLE, { x: 0.75, y: ny, w: 0.08, h: nh, fill: { color: COLOR.PRIMARY }, line: { type: "none" } });
  s.addText([
    { text: "설계 포인트   ", options: { fontFace: FONT.XBOLD, color: COLOR.PRIMARY } },
    { text: "읽기·쓰기마다 짧은 수명의 ", options: { fontFace: FONT.MED, color: COLOR.SUB } },
    { text: "TcAdsClient", options: { fontFace: FONT.BOLD, color: COLOR.INK } },
    { text: " 로 연결하고, DO 비트 매핑(가동·공정·경고·AI)은 변경 불가한 하드웨어 계약으로 고정", options: { fontFace: FONT.MED, color: COLOR.SUB } },
  ], { x: 1.12, y: ny, w: 11.4, h: nh, margin: 0, fontSize: 12.5, align: "left", valign: "middle" });

  T.footer(pres, s, { page: 10, total: TOTAL });
}

// ── 슬라이드 11: 센서 폴링 & 보정 (파이프라인) ─────────────
function s11_polling() {
  const s = newSlide();
  T.header(pres, s, { kicker: "Part 4 · 핵심 기능 구현", title: "센서 폴링 & 보정" });
  s.addText("DispatcherTimer가 약 1Hz로 돌며 원시값을 읽어 공학값으로 환산하고, 정지(stale)를 감지한 뒤 평가·저장합니다.", {
    x: 0.75, y: 1.5, w: 11.9, h: 0.4, margin: 0, fontFace: FONT.MED, fontSize: 13.5, color: COLOR.SUB, valign: "middle",
  });

  // 파이프라인 5단계
  const py = 2.05, ph = 1.12, pw = 2.05, pgap = 0.36;
  const steps = [
    ["타이머", "DispatcherTimer\n약 1Hz 폴링"],
    ["ADS 읽기", "AD4203 16B→Int16×4\nID5342 비트"],
    ["보정·환산", "선형식으로\nraw → 공학값"],
    ["stale 감지", "6회 연속 동일값\n→ 정지 의심"],
    ["평가·저장", "위험 평가 +\n스냅샷·Flask 전송"],
  ];
  steps.forEach(([t, sub], i) => {
    const x = 0.75 + i * (pw + pgap);
    s.addShape(pres.shapes.RECTANGLE, { x, y: py, w: pw, h: ph, rectRadius: 0.06, fill: { color: COLOR.BG }, line: { color: COLOR.PRIMARY, width: 1.25 }, shadow: T.shadow({ blur: 5, offset: 2, opacity: 0.07 }) });
    s.addShape(pres.shapes.OVAL, { x: x + pw / 2 - 0.15, y: py - 0.13, w: 0.3, h: 0.3, fill: { color: COLOR.PRIMARY }, line: { color: COLOR.BG, width: 1.5 } });
    s.addText(String(i + 1), { x: x + pw / 2 - 0.15, y: py - 0.13, w: 0.3, h: 0.3, margin: 0, fontFace: FONT.XBOLD, fontSize: 11, color: "FFFFFF", align: "center", valign: "middle" });
    s.addText(t, { x, y: py + 0.22, w: pw, h: 0.32, margin: 0, fontFace: FONT.BOLD, fontSize: 13, color: COLOR.INK, align: "center", valign: "middle" });
    s.addText(sub, { x: x + 0.1, y: py + 0.56, w: pw - 0.2, h: 0.5, margin: 0, fontFace: FONT.REG, fontSize: 9.5, color: COLOR.SUB, align: "center", valign: "top", lineSpacingMultiple: 1.08 });
    if (i < steps.length - 1) {
      s.addShape(pres.shapes.LINE, { x: x + pw + 0.04, y: py + ph / 2, w: pgap - 0.08, h: 0, line: { color: COLOR.PRIMARY_DK, width: 2, endArrowType: "triangle" } });
    }
  });

  // 보정식 패널
  const calY = 3.62, calX = 0.75, calW = 7.45, calH = 2.9;
  s.addShape(pres.shapes.RECTANGLE, { x: calX, y: calY, w: calW, h: calH, fill: { color: COLOR.BG }, line: { color: COLOR.BORDER, width: 1 }, shadow: T.shadow({ blur: 6, offset: 2, opacity: 0.08 }) });
  s.addShape(pres.shapes.RECTANGLE, { x: calX, y: calY, w: calW, h: 0.5, fill: { color: COLOR.PRIMARY }, line: { type: "none" } });
  s.addText("보정식 · 원시 Int16 → 공학값 (선형 환산)", { x: calX + 0.28, y: calY, w: calW - 0.5, h: 0.5, margin: 0, fontFace: FONT.BOLD, fontSize: 12.5, color: "FFFFFF", valign: "middle" });
  const cal = [
    ["압력", "raw × 0.000161 − 0.153", "bar"],
    ["진동", "raw × 0.001282 + 0.051", "level · 0~10 클램프"],
    ["온도", "raw × 0.014035 − 39.56", "℃"],
    ["습도", "raw × 0.010345 + 5.793", "%"],
  ];
  const crY = calY + 0.74, crH = (calH - 0.92) / 4;
  cal.forEach(([name, formula, unit], i) => {
    const ry = crY + i * crH;
    s.addText(name, { x: calX + 0.3, y: ry, w: 0.9, h: crH, margin: 0, fontFace: FONT.SEMI, fontSize: 13, color: COLOR.INK, valign: "middle" });
    s.addText(formula, { x: calX + 1.25, y: ry, w: 3.6, h: crH, margin: 0, fontFace: FONT.MED, fontSize: 12.5, color: COLOR.PRIMARY_DK, valign: "middle" });
    s.addText(unit, { x: calX + 4.95, y: ry, w: 2.35, h: crH, margin: 0, fontFace: FONT.REG, fontSize: 11, color: COLOR.SUB, valign: "middle" });
    if (i < 3) s.addShape(pres.shapes.LINE, { x: calX + 0.3, y: ry + crH, w: calW - 0.6, h: 0, line: { color: COLOR.BORDER, width: 0.75 } });
  });

  // 우측 정보 카드 2개
  const ix = 8.55, iw = 4.1, ih = 1.38, igap = 0.14, iy = 3.62;
  T.accentCard(pres, s, { x: ix, y: iy, w: iw, h: ih, accent: COLOR.AMBER, kicker: "STALE 감지", title: "정지 의심 보류", desc: "6회 연속 동일값이면 누적 보류" });
  T.accentCard(pres, s, { x: ix, y: iy + ih + igap, w: iw, h: ih, accent: COLOR.GREEN, kicker: "저장 · 전송", title: "영속화 + 텔레메트리", desc: "주기 저장 + Flask 비차단 전송" });

  T.footer(pres, s, { page: 11, total: TOTAL });
}

s01_cover();
s02_agenda();
s03_overview();
s04_background();
s05_goals();
s06_requirements();
s07_stack();
s08_architecture();
s09_decisions();
// ── 슬라이드 12: 위험 엔진 ──────────────────────────────────
function s12_risk() {
  const s = newSlide();
  T.header(pres, s, { kicker: "Part 4 · 핵심 기능 구현", title: "위험 엔진 (Risk Engine)" });
  s.addText([
    { text: "센서값을 임계값과 비교해 위험을 누적·판정하고,", options: { breakLine: true } },
    { text: "한도 도달 시 공정을 자동으로 종료합니다." },
  ], { x: 0.75, y: 1.55, w: 5.6, h: 0.6, margin: 0, fontFace: FONT.MED, fontSize: 14, color: COLOR.SUB, valign: "top", lineSpacingMultiple: 1.1 });

  T.numberedRows(pres, s, {
    x: 0.75, startY: 2.5, items: [
      ["임계 평가", "압력·진동·온도·습도를 설정 임계값과 비교"],
      ["슬라이딩 윈도우 누적", "RiskWindowSeconds(기본 60초) 내 위험 이벤트를 누적·리셋"],
      ["자동 종료", "누적이 한도(AutoShutdownWarningLimit) 도달 시 공정 정지 + 경고 램프"],
      ["노이즈 방지", "해제 3초 디바운스 · 10초 지속마다 +1 · stale 시 누적 보류"],
    ],
  });
  T.screenshotCard(pres, s, { path: SHOT.danger, x: RIGHT_CARD.x, y: RIGHT_CARD.y, w: RIGHT_CARD.w, imgRatio: SHOT_RATIO, inset: RIGHT_CARD.inset });
  T.captionBar(pres, s, {
    x: RIGHT_CARD.x, y: 5.598, w: RIGHT_CARD.w, accent: COLOR.RED,
    runs: [
      { text: "진동 9.5 기준 초과 · 60초 내 위험 카운트 2/2", options: { fontFace: FONT.SEMI, color: COLOR.INK } },
      { text: "  →  공정 자동 정지", options: { fontFace: FONT.SEMI, color: COLOR.RED } },
    ],
  });
  T.footer(pres, s, { page: 12, total: TOTAL });
}

// ── 슬라이드 13: 램프 · 공정 제어 & 인터락 ─────────────────
function s13_control() {
  const s = newSlide();
  T.header(pres, s, { kicker: "Part 4 · 핵심 기능 구현", title: "램프 · 공정 제어 & 인터락" });
  s.addText("디지털 출력 4비트로 램프를 제어하고, FOUP·권한·위험 조건으로 공정을 인터락합니다.", {
    x: 0.75, y: 1.5, w: 11.9, h: 0.4, margin: 0, fontFace: FONT.MED, fontSize: 13.5, color: COLOR.SUB, valign: "middle",
  });

  // DO 비트맵 strip
  const sy = 2.05, sh = 1.32, sw = 2.83, sgap = 0.2;
  const bits = [
    ["DO1 · bit 0", "가동 램프", COLOR.GREEN, "장비 가동 표시 · 펄스 출력 예약"],
    ["DO2 · bit 1", "공정 램프", COLOR.PRIMARY, "공정 진행 중 점등"],
    ["DO3 · bit 2", "경고·위험 램프", COLOR.RED, "위험·경고 상태에서 점등"],
    ["DO4 · bit 3", "AI 제어 램프", COLOR.PRIMARY_DK, "AI 자동제어 동작 표시"],
  ];
  bits.forEach(([label, name, dot, note], i) => {
    const x = 0.75 + i * (sw + sgap);
    s.addShape(pres.shapes.RECTANGLE, { x, y: sy, w: sw, h: sh, fill: { color: COLOR.BG }, line: { color: COLOR.BORDER, width: 1 }, shadow: T.shadow({ blur: 5, offset: 2, opacity: 0.07 }) });
    s.addText(label, { x: x + 0.22, y: sy + 0.16, w: sw - 0.4, h: 0.26, margin: 0, fontFace: FONT.SEMI, fontSize: 10, color: COLOR.FAINT, charSpacing: 1, valign: "middle" });
    s.addShape(pres.shapes.OVAL, { x: x + 0.24, y: sy + 0.56, w: 0.18, h: 0.18, fill: { color: dot }, line: { type: "none" } });
    s.addText(name, { x: x + 0.52, y: sy + 0.48, w: sw - 0.7, h: 0.34, margin: 0, fontFace: FONT.BOLD, fontSize: 14, color: COLOR.INK, valign: "middle" });
    s.addText(note, { x: x + 0.22, y: sy + 0.9, w: sw - 0.42, h: 0.36, margin: 0, fontFace: FONT.REG, fontSize: 10.5, color: COLOR.SUB, valign: "top", lineSpacingMultiple: 1.1 });
  });

  // 인터락 규칙 카드 3
  const iy = 3.62, iw = 3.83, igap = 0.2, ih = 2.0;
  const inter = [
    [COLOR.GREEN, "FOUP A·B 게이팅", "광센서 + 근접센서가 모두 ON일 때만 공정 진행. 진행 중 하나라도 해제되면 안전을 위해 자동 정지."],
    [COLOR.RED, "강제 정지", "위험 한도 도달 또는 수동 명령 시 공정을 즉시 중단하고 경고·위험 램프(DO3)를 점등."],
    [COLOR.PRIMARY, "권한 게이팅", "로그인 여부와 권한(Operator·Admin)에 따라 제어·설정 버튼을 활성·차단."],
  ];
  inter.forEach(([accent, title, desc], i) => {
    T.accentCard(pres, s, { x: 0.75 + i * (iw + igap), y: iy, w: iw, h: ih, accent, title, desc });
  });

  // 하단 노트
  const ny = 5.85, nh = 0.78;
  s.addShape(pres.shapes.RECTANGLE, { x: 0.75, y: ny, w: 11.9, h: nh, fill: { color: COLOR.PANEL }, line: { type: "none" } });
  s.addShape(pres.shapes.RECTANGLE, { x: 0.75, y: ny, w: 0.08, h: nh, fill: { color: COLOR.PRIMARY }, line: { type: "none" } });
  s.addText([
    { text: "공정 시작 조건   ", options: { fontFace: FONT.XBOLD, color: COLOR.PRIMARY } },
    { text: "FOUP A·B 감지 + 승인된 사용자 로그인이 모두 충족될 때만 공정을 시작할 수 있다.", options: { fontFace: FONT.MED, color: COLOR.SUB } },
  ], { x: 1.12, y: ny, w: 11.4, h: nh, margin: 0, fontSize: 12.5, align: "left", valign: "middle" });

  T.footer(pres, s, { page: 13, total: TOTAL });
}

// ── 슬라이드 14: AI 자동제어 ────────────────────────────────
function s14_ai() {
  const s = newSlide();
  T.header(pres, s, { kicker: "Part 4 · 핵심 기능 구현", title: "AI 자동제어" });
  s.addText([
    { text: "AI 자동제어가 켜지면 임계를 초과한 측정값을", options: { breakLine: true } },
    { text: "임계값의 95%로 보정해 공정을 유지합니다." },
  ], { x: 0.75, y: 1.55, w: 5.4, h: 0.6, margin: 0, fontFace: FONT.MED, fontSize: 14, color: COLOR.SUB, valign: "top", lineSpacingMultiple: 1.1 });

  T.numberedRows(pres, s, {
    x: 0.75, startY: 2.5, descW: 4.6, items: [
      ["활성화", "DI3(AI 시작)로 ON · AI 제어 램프(DO4) 점등"],
      ["초과 감지", "압력·진동·온도·습도가 임계를 넘는지 매 폴링 확인"],
      ["값 보정", "초과 채널을 임계값의 95%로 끌어내림(ApplyAiControlCorrection)"],
      ["공정 유지", "위험 누적을 회피하고 공정을 계속 진행"],
    ],
  });

  // 우측: 공식 콜아웃 + 보류 노트
  const rx = 5.95, rw = 6.75;
  s.addShape(pres.shapes.RECTANGLE, { x: rx, y: 2.1, w: rw, h: 1.95, fill: { color: COLOR.PANEL }, line: { type: "none" }, shadow: T.shadow({ blur: 6, offset: 2, opacity: 0.08 }) });
  s.addText("보정값 = 임계값 × 0.95", { x: rx, y: 2.45, w: rw, h: 0.85, margin: 0, fontFace: FONT.XBOLD, fontSize: 30, color: COLOR.PRIMARY, align: "center", valign: "middle" });
  s.addText("AI 자동제어 ON · 임계를 초과한 채널에만 적용", { x: rx, y: 3.3, w: rw, h: 0.4, margin: 0, fontFace: FONT.MED, fontSize: 12.5, color: COLOR.SUB, align: "center", valign: "middle" });

  s.addShape(pres.shapes.RECTANGLE, { x: rx, y: 4.3, w: rw, h: 2.1, fill: { color: "FBF6E8" }, line: { color: COLOR.AMBER, width: 1 } });
  s.addShape(pres.shapes.RECTANGLE, { x: rx, y: 4.3, w: 0.08, h: 2.1, fill: { color: COLOR.AMBER }, line: { type: "none" } });
  s.addText("트레이너 데모용 설계", { x: rx + 0.34, y: 4.5, w: rw - 0.6, h: 0.4, margin: 0, fontFace: FONT.BOLD, fontSize: 15, color: "9A6B00", valign: "middle" });
  s.addText("AI 보정은 실제 안전 인터록을 대체하지 않습니다. 하드웨어 제어 한계상 인터록 우회는 학습 목적의 의도된 동작이며, 실장비 운용에는 적용하지 않습니다.", {
    x: rx + 0.34, y: 4.95, w: rw - 0.66, h: 1.3, margin: 0, fontFace: FONT.MED, fontSize: 12.5, color: COLOR.SUB, align: "left", valign: "top", lineSpacingMultiple: 1.22,
  });

  T.footer(pres, s, { page: 14, total: TOTAL });
}

// ── 슬라이드 15: 인증 & 권한 (Flask) ───────────────────────
function s15_auth() {
  const s = newSlide();
  T.header(pres, s, { kicker: "Part 4 · 핵심 기능 구현", title: "인증 & 권한 (Flask)" });
  s.addText([
    { text: "Flask 백엔드로 회원가입·승인·로그인을 처리하고,", options: { breakLine: true } },
    { text: "권한으로 민감한 조작을 통제합니다." },
  ], { x: 0.75, y: 1.55, w: 5.5, h: 0.6, margin: 0, fontFace: FONT.MED, fontSize: 14, color: COLOR.SUB, valign: "top", lineSpacingMultiple: 1.1 });

  T.numberedRows(pres, s, {
    x: 0.75, startY: 2.55, items: [
      ["회원가입 요청", "가입 시 Pending 상태로 대기(승인 전 로그인 불가)"],
      ["관리자 승인", "Admin이 가입 요청을 승인·거절로 관리"],
      ["로그인", "FlaskAuthService · HttpWebRequest, 5초 타임아웃(동기)"],
      ["권한 게이팅", "Operator·Admin 역할로 제어·설정 접근을 제한"],
    ],
  });

  const ratio = 2560 / 1392;
  const cy = 1.32;
  const ch = T.screenshotCard(pres, s, { path: "flask-3_s.png", x: RIGHT_CARD.x, y: cy, w: RIGHT_CARD.w, imgRatio: ratio, inset: RIGHT_CARD.inset });
  T.captionBar(pres, s, {
    x: RIGHT_CARD.x, y: cy + ch + 0.18, w: RIGHT_CARD.w, accent: COLOR.PRIMARY,
    runs: [
      { text: "사용자 승인", options: { fontFace: FONT.SEMI, color: COLOR.INK } },
      { text: ",  가입 요청 승인·거절 → 승인 시 로그인 허용", options: { fontFace: FONT.MED, color: COLOR.SUB } },
    ],
  });
  T.footer(pres, s, { page: 15, total: TOTAL });
}

// ── 슬라이드 16: 데이터 & 텔레메트리 (Flask) ───────────────
function s16_data() {
  const s = newSlide();
  T.header(pres, s, { kicker: "Part 4 · 핵심 기능 구현", title: "데이터 & 텔레메트리 (Flask)" });
  s.addText([
    { text: "모든 텔레메트리를 로컬 SQLite에 영속화하고,", options: { breakLine: true } },
    { text: "Flask로 미러링해 원격에서 감시합니다." },
  ], { x: 0.75, y: 1.55, w: 5.5, h: 0.6, margin: 0, fontFace: FONT.MED, fontSize: 14, color: COLOR.SUB, valign: "top", lineSpacingMultiple: 1.1 });

  T.numberedRows(pres, s, {
    x: 0.75, startY: 2.55, items: [
      ["로컬 영속화", "SQLite(WAL)에 센서 스냅샷·활동 로그 기록"],
      ["비차단 전송", "Flask로 fire-and-forget, ~1/초 자가 throttle"],
      ["원격 모니터링", "관리자 대시보드에서 실시간 센서·로그 확인"],
      ["장비 상태 인식", "RUNNING·정지 등 장비 상태를 함께 전송·표시"],
    ],
  });

  const ratio = 2560 / 1392;
  const cy = 1.32;
  const ch = T.screenshotCard(pres, s, { path: "flask-1_s.png", x: RIGHT_CARD.x, y: cy, w: RIGHT_CARD.w, imgRatio: ratio, inset: RIGHT_CARD.inset });
  T.captionBar(pres, s, {
    x: RIGHT_CARD.x, y: cy + ch + 0.18, w: RIGHT_CARD.w, accent: COLOR.GREEN,
    runs: [
      { text: "관리자 대시보드", options: { fontFace: FONT.SEMI, color: COLOR.INK } },
      { text: ",  실시간 센서 + 장비 상태(RUNNING/정지) 원격 확인", options: { fontFace: FONT.MED, color: COLOR.SUB } },
    ],
  });
  T.footer(pres, s, { page: 16, total: TOTAL });
}

s10_hardware();
s11_polling();
s12_risk();
s13_control();
s14_ai();
// ── 슬라이드 17: 데모 ① 위험 자동 정지 시나리오 ───────────
function s17_demo_flow() {
  const s = newSlide();
  T.header(pres, s, { kicker: "Part 5 · 검증 & 데모", title: "데모 ① 위험 자동 정지 시나리오" });
  s.addText("실제 실행에서 정상 운전이 경고를 거쳐 사람 개입 없이 자동 정지에 이르는 과정입니다.", {
    x: 0.75, y: 1.5, w: 11.9, h: 0.4, margin: 0, fontFace: FONT.MED, fontSize: 13.5, color: COLOR.SUB, valign: "middle",
  });

  const ratio = 1426 / 893;
  const cw = 3.85, cgap = 0.2, cy = 2.35;
  const frames = [
    ["app_normal_s.png", "1", COLOR.GREEN, "정상", "모든 센서 정상 · AI 감시 중"],
    ["app_warn_s.png", "2", COLOR.AMBER, "경고", "진동 9.8 초과 · 위험 1/2"],
    ["app_danger_s.png", "3", COLOR.RED, "자동 정지", "위험 2/2 → 공정 정지"],
  ];
  frames.forEach(([path, num, accent, status, detail], i) => {
    const x = 0.75 + i * (cw + cgap);
    const ch = T.screenshotCard(pres, s, { path, x, y: cy, w: cw, imgRatio: ratio, inset: 0.09 });
    // 단계 배지
    s.addShape(pres.shapes.OVAL, { x: x - 0.08, y: cy - 0.16, w: 0.4, h: 0.4, fill: { color: accent }, line: { color: COLOR.BG, width: 2 } });
    s.addText(num, { x: x - 0.08, y: cy - 0.16, w: 0.4, h: 0.4, margin: 0, fontFace: FONT.XBOLD, fontSize: 15, color: "FFFFFF", align: "center", valign: "middle" });
    // 상태 캡션
    T.captionBar(pres, s, {
      x, y: cy + ch + 0.14, w: cw, accent,
      runs: [
        { text: status, options: { fontFace: FONT.BOLD, color: accent } },
        { text: "  " + detail, options: { fontFace: FONT.MED, color: COLOR.SUB } },
      ],
    });
  });

  // 하단 takeaway
  const ny = 5.78, nh = 0.82;
  s.addShape(pres.shapes.RECTANGLE, { x: 0.75, y: ny, w: 11.9, h: nh, fill: { color: COLOR.PANEL }, line: { type: "none" } });
  s.addShape(pres.shapes.RECTANGLE, { x: 0.75, y: ny, w: 0.08, h: nh, fill: { color: COLOR.RED }, line: { type: "none" } });
  s.addText([
    { text: "검증 결과   ", options: { fontFace: FONT.XBOLD, color: COLOR.RED } },
    { text: "센서 위험 → 60초 윈도우 누적 → 한도(2/2) 도달 시 ", options: { fontFace: FONT.MED, color: COLOR.SUB } },
    { text: "사람 개입 없이 공정 자동 정지", options: { fontFace: FONT.BOLD, color: COLOR.INK } },
    { text: " 를 실제로 확인", options: { fontFace: FONT.MED, color: COLOR.SUB } },
  ], { x: 1.12, y: ny, w: 11.4, h: nh, margin: 0, fontSize: 12.5, align: "left", valign: "middle" });

  T.footer(pres, s, { page: 17, total: TOTAL });
}

// ── 슬라이드 18: 데모 ② 원격 검증 · Flask 로그 ─────────────
function s18_demo_remote() {
  const s = newSlide();
  T.header(pres, s, { kicker: "Part 5 · 검증 & 데모", title: "데모 ② 원격 검증 · Flask 로그" });
  s.addText("동일한 이벤트가 로컬 SQLite와 Flask에 함께 기록되어, 장비 → 로컬 → 원격까지 끊김 없이 추적됩니다.", {
    x: 0.75, y: 1.5, w: 11.9, h: 0.4, margin: 0, fontFace: FONT.MED, fontSize: 13.5, color: COLOR.SUB, valign: "middle",
  });

  const ratio = 2560 / 1392;
  const cw = 5.85, cgap = 0.25, cy = 2.55;
  const cards = [
    ["flask-4_s.png", COLOR.PRIMARY, "센서 로그", "시간별 센서값 + 위험 등급 + 장비 상태(RUNNING)"],
    ["flask-2_s.png", COLOR.GREEN, "행동 로그", "제어·인증 이벤트를 INFO · WARN · RISK 등급으로"],
  ];
  cards.forEach(([path, accent, title, detail], i) => {
    const x = 0.75 + i * (cw + cgap);
    const ch = T.screenshotCard(pres, s, { path, x, y: cy, w: cw, imgRatio: ratio, inset: 0.09 });
    T.captionBar(pres, s, {
      x, y: cy + ch + 0.16, w: cw, accent,
      runs: [
        { text: title, options: { fontFace: FONT.BOLD, color: COLOR.INK } },
        { text: "  " + detail, options: { fontFace: FONT.MED, color: COLOR.SUB } },
      ],
    });
  });

  T.footer(pres, s, { page: 18, total: TOTAL });
}

s15_auth();
s16_data();
// ── 슬라이드 19: 트러블슈팅 · 회고 (Before → After) ────────
function s19_retro() {
  const s = newSlide();
  T.header(pres, s, { kicker: "Part 6 · 회고 & 향후", title: "트러블슈팅 · 회고" });
  s.addText("실제로 부딪힌 문제를 기존 동작에서 개선 동작으로 다듬었습니다.", {
    x: 0.75, y: 1.5, w: 11.9, h: 0.4, margin: 0, fontFace: FONT.MED, fontSize: 13.5, color: COLOR.SUB, valign: "middle",
  });

  const CX = 0.75, CW = 11.9;
  const TITLE_X = CX + 0.95, B_X = CX + 4.35, A_X = CX + 8.3;
  const DIV1 = CX + 4.05, DIV2 = CX + 8.0;
  const B_W = 3.7, A_W = 3.45;
  [["문제", TITLE_X], ["기존 (Before)", B_X], ["개선 (After)", A_X]].forEach(([t, x]) => {
    s.addText(t.toUpperCase(), { x, y: 2.0, w: 3.6, h: 0.24, margin: 0, fontFace: FONT.SEMI, fontSize: 10, color: COLOR.FAINT, charSpacing: 1.2, valign: "middle" });
  });

  const rows = [
    ["1", "위험 누적 노이즈", "경고 깜빡임마다 중복 누적·즉시 리셋", "3초 디바운스 · 10초 지속마다 +1 누적"],
    ["2", "FOUP 해제 중 공정", "FOUP A·B가 빠져도 공정이 계속 진행", "진행 중 FOUP 해제 시 공정 자동 정지"],
    ["3", "인증 없는 제어", "로그인과 무관하게 장비 조작 가능", "권한 게이팅 · 로그아웃 시 공정 정지"],
    ["4", "상태 표시 혼동", "인증 결과와 API 연결을 한 덩어리로 표시", "인증 결과와 연결 상태를 분리 표시"],
  ];
  const RY = 2.32, RH = 1.0, GAP = 0.13;
  rows.forEach(([num, title, before, after], i) => {
    const y = RY + i * (RH + GAP);
    s.addShape(pres.shapes.RECTANGLE, { x: CX, y, w: CW, h: RH, fill: { color: COLOR.PANEL }, line: { type: "none" }, shadow: T.shadow({ blur: 5, offset: 2, opacity: 0.06 }) });
    s.addShape(pres.shapes.RECTANGLE, { x: CX, y, w: 0.08, h: RH, fill: { color: COLOR.GREEN }, line: { type: "none" } });
    const bs = 0.42, by = y + (RH - bs) / 2;
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: CX + 0.32, y: by, w: bs, h: bs, rectRadius: 0.07, fill: { color: COLOR.GREEN }, line: { type: "none" } });
    s.addText(num, { x: CX + 0.32, y: by, w: bs, h: bs, margin: 0, fontFace: FONT.XBOLD, fontSize: 15, color: "FFFFFF", align: "center", valign: "middle" });
    s.addText(title, { x: TITLE_X, y, w: 2.95, h: RH, margin: 0, fontFace: FONT.BOLD, fontSize: 13.5, color: COLOR.INK, valign: "middle" });
    s.addShape(pres.shapes.LINE, { x: DIV1, y: y + 0.18, w: 0, h: RH - 0.36, line: { color: COLOR.BORDER, width: 1 } });
    s.addShape(pres.shapes.LINE, { x: DIV2, y: y + 0.18, w: 0, h: RH - 0.36, line: { color: COLOR.BORDER, width: 1 } });
    s.addText(before, { x: B_X, y, w: B_W, h: RH, margin: 0, fontFace: FONT.MED, fontSize: 11.5, color: COLOR.SUB, valign: "middle", lineSpacingMultiple: 1.12 });
    s.addText(after, { x: A_X, y, w: A_W, h: RH, margin: 0, fontFace: FONT.SEMI, fontSize: 11.5, color: COLOR.GREEN, valign: "middle", lineSpacingMultiple: 1.12 });
  });

  T.footer(pres, s, { page: 19, total: TOTAL });
}

// ── 슬라이드 20: 팀 역할분담 & 협업 ────────────────────────
function s20_team() {
  const s = newSlide();
  T.header(pres, s, { kicker: "Part 6 · 회고 & 향후", title: "팀 역할분담 & 협업" });
  s.addText("3인 팀이 클라이언트와 백엔드로 역할을 나눠 API 계약 기반으로 협업했습니다.", {
    x: 0.75, y: 1.5, w: 11.9, h: 0.4, margin: 0, fontFace: FONT.MED, fontSize: 13.5, color: COLOR.SUB, valign: "middle",
  });

  const members = [
    [COLOR.PRIMARY, "박재민", "클라이언트 · WPF", "UI · 센서/위험 엔진 · TwinCAT ADS 연동 · 로컬 SQLite"],
    [COLOR.GREEN, "김길동 · 팀장", "백엔드 · Flask", "인증 · 사용자 승인 · 관리자 콘솔 화면"],
    [COLOR.GREEN, "최현규", "백엔드 · Flask", "텔레메트리 수신 · 센서/행동 로그 · 대시보드"],
  ];
  const cw = 3.83, cgap = 0.2, cy = 2.35, ch = 2.55;
  members.forEach(([accent, name, role, area], i) => {
    T.accentCard(pres, s, { x: 0.75 + i * (cw + cgap), y: cy, w: cw, h: ch, accent, kicker: name, title: role, desc: area });
  });

  const ny = 5.35, nh = 0.85;
  s.addShape(pres.shapes.RECTANGLE, { x: 0.75, y: ny, w: 11.9, h: nh, fill: { color: COLOR.PANEL }, line: { type: "none" } });
  s.addShape(pres.shapes.RECTANGLE, { x: 0.75, y: ny, w: 0.08, h: nh, fill: { color: COLOR.PRIMARY }, line: { type: "none" } });
  s.addText([
    { text: "협업 방식   ", options: { fontFace: FONT.XBOLD, color: COLOR.PRIMARY } },
    { text: "Git 브랜치 기반 분업 · REST API 계약으로 클라이언트와 백엔드를 독립 개발·통합", options: { fontFace: FONT.MED, color: COLOR.SUB } },
  ], { x: 1.12, y: ny, w: 11.4, h: nh, margin: 0, fontSize: 12.5, align: "left", valign: "middle" });

  T.footer(pres, s, { page: 20, total: TOTAL });
}

// ── 슬라이드 21: 한계 & 향후 개선 ──────────────────────────
function s21_future() {
  const s = newSlide();
  T.header(pres, s, { kicker: "Part 6 · 회고 & 향후", title: "한계 & 향후 개선" });
  s.addText("학습용 트레이너로서의 한계를 인지하고, 다음 단계 개선 방향을 정리했습니다.", {
    x: 0.75, y: 1.5, w: 11.9, h: 0.4, margin: 0, fontFace: FONT.MED, fontSize: 13.5, color: COLOR.SUB, valign: "middle",
  });

  const panelW = 5.77, gapX = 0.41, panelY = 2.25, panelH = 4.3, panelX2 = 0.75 + panelW + gapX;
  const panel = (x, accent, label, rows) => {
    s.addShape(pres.shapes.RECTANGLE, { x, y: panelY, w: panelW, h: panelH, fill: { color: COLOR.BG }, line: { color: COLOR.BORDER, width: 1 }, shadow: T.shadow({ blur: 6, offset: 2, opacity: 0.08 }) });
    s.addShape(pres.shapes.RECTANGLE, { x, y: panelY, w: panelW, h: 0.62, fill: { color: accent }, line: { type: "none" } });
    s.addText(label, { x: x + 0.34, y: panelY, w: panelW - 0.5, h: 0.62, margin: 0, fontFace: FONT.BOLD, fontSize: 15, color: "FFFFFF", valign: "middle" });
    const rowY = panelY + 0.95, rowH = (panelH - 1.15) / rows.length;
    rows.forEach(([term, desc], i) => {
      T.checkRow(pres, s, { x: x + 0.34, y: rowY + i * rowH, w: panelW - 0.6, accent, term, desc });
    });
  };
  panel(0.75, COLOR.RED, "현재 한계", [
    ["AI 인터록 우회", "데모용 · 실제 안전장치 아님"],
    ["통신 보안", "HTTP 평문 전송"],
    ["자동화 테스트", "단위·통합 테스트 부재"],
    ["단일 장비", "트레이너 1대 대상"],
  ]);
  panel(panelX2, COLOR.GREEN, "향후 개선", [
    ["보안 강화", "HTTPS · 토큰 인증"],
    ["테스트 도입", "위험 엔진 단위 테스트"],
    ["다중 장비", "N대 동시 감시·대시보드"],
    ["임계 자동 튜닝", "수집 데이터 기반 보정"],
  ]);

  T.footer(pres, s, { page: 21, total: TOTAL });
}

// ── 슬라이드 22: 마무리 / Q&A (다크 클로징) ────────────────
function s22_closing() {
  const s = pres.addSlide();
  s.background = { color: "0E2440" };
  s.addShape(pres.shapes.RECTANGLE, { x: 0, y: 0, w: 0.16, h: DECK.H, fill: { color: COLOR.PRIMARY }, line: { type: "none" } });

  const LX = 0.95;
  s.addText("반도체 장비 센서 제어 시스템", { x: LX, y: 2.05, w: 10, h: 0.3, margin: 0, fontFace: FONT.SEMI, fontSize: 13, color: "9CC2F0", charSpacing: 2, valign: "middle" });
  s.addText("감사합니다", { x: LX - 0.02, y: 2.45, w: 11, h: 1.3, margin: 0, fontFace: FONT.BLACK, fontSize: 54, color: "FFFFFF", valign: "middle" });
  s.addText("실 장비 없이 센서 감시 · 위험 자동 대응 · 이력 · 원격 연동을 한 흐름으로 검증했습니다.", {
    x: LX, y: 3.95, w: 10.5, h: 0.5, margin: 0, fontFace: FONT.MED, fontSize: 15, color: "C9D6E6", valign: "middle",
  });

  // 기술 칩(다크)
  const chips = [["WPF · .NET", 1.55], ["TwinCAT ADS", 1.75], ["SQLite", 1.05], ["Flask", 0.95]];
  let cx = LX;
  chips.forEach(([text, w]) => {
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: cx, y: 4.75, w, h: 0.44, rectRadius: 0.08, fill: { color: "163255" }, line: { color: "2C4A72", width: 1 } });
    s.addText(text, { x: cx, y: 4.75, w, h: 0.44, margin: 0, fontFace: FONT.SEMI, fontSize: 11.5, color: "BFD4EE", align: "center", valign: "middle" });
    cx += w + 0.18;
  });

  s.addText("Q & A", { x: LX, y: 5.75, w: 4, h: 0.5, margin: 0, fontFace: FONT.XBOLD, fontSize: 20, color: "FFFFFF", valign: "middle" });
  s.addShape(pres.shapes.LINE, { x: LX, y: 6.35, w: 11.45, h: 0, line: { color: "27456B", width: 1 } });
  s.addText("교육과정 최종 프로젝트 · 2026. 06. 18", { x: LX, y: 6.45, w: 7, h: 0.3, margin: 0, fontFace: FONT.MED, fontSize: 10.5, color: "7E96B4", valign: "middle" });
  s.addText("22 / " + TOTAL, { x: DECK.W - DECK.MR - 1.5, y: 6.45, w: 1.5, h: 0.3, margin: 0, fontFace: FONT.SEMI, fontSize: 10.5, color: "7E96B4", align: "right", valign: "middle" });
}

s17_demo_flow();
s18_demo_remote();
// ── 슬라이드 23(부록): 스코프 — 의도적으로 제외한 것 ───────
function s23_scope_appendix() {
  const s = newSlide();
  T.header(pres, s, { kicker: "부록 · Appendix", title: "스코프 · 의도적으로 제외한 것" });
  s.addText("센서 감시·위험 대응 트레이너에 집중하기 위해, 장비 거동 시뮬레이션은 범위에서 제외했습니다.", {
    x: 0.75, y: 1.5, w: 11.9, h: 0.4, margin: 0, fontFace: FONT.MED, fontSize: 13.5, color: COLOR.SUB, valign: "middle",
  });

  const cw = 3.83, cgap = 0.2, cy = 2.35, ch = 3.05;
  const cols = [
    [COLOR.AMBER, "제외한 것", "FOUP 도킹·이송 동작, TM(이송 모듈)·PM(공정 모듈)의 실제 거동 애니메이션, 챔버 공정 진행 시각화."],
    [COLOR.PRIMARY, "그 이유", "목표는 센서 감시·위험 대응 트레이너. 장비 거동 시뮬레이션은 범위 밖이라, 복잡도와 산만함을 줄여 핵심에 집중했습니다."],
    [COLOR.GREEN, "대신 한 것", "FOUP A·B는 광·근접 센서 인터락 신호로만 모델링하고 포트·챔버는 상태 표시 수준. 신호 기반 공정 게이팅·자동 정지는 실제로 동작합니다."],
  ];
  cols.forEach(([accent, title, desc], i) => {
    T.accentCard(pres, s, { x: 0.75 + i * (cw + cgap), y: cy, w: cw, h: ch, accent, title, desc });
  });

  const ny = 5.7, nh = 0.78;
  s.addShape(pres.shapes.RECTANGLE, { x: 0.75, y: ny, w: 11.9, h: nh, fill: { color: COLOR.PANEL }, line: { type: "none" } });
  s.addShape(pres.shapes.RECTANGLE, { x: 0.75, y: ny, w: 0.08, h: nh, fill: { color: COLOR.PRIMARY }, line: { type: "none" } });
  s.addText([
    { text: "한 줄 요약   ", options: { fontFace: FONT.XBOLD, color: COLOR.PRIMARY } },
    { text: "장비 거동 시뮬은 ", options: { fontFace: FONT.MED, color: COLOR.SUB } },
    { text: "스코프 밖", options: { fontFace: FONT.BOLD, color: COLOR.INK } },
    { text: " — 센서·위험·제어 신호 로직은 실제로 구현했습니다.", options: { fontFace: FONT.MED, color: COLOR.SUB } },
  ], { x: 1.12, y: ny, w: 11.4, h: nh, margin: 0, fontSize: 12.5, align: "left", valign: "middle" });

  // 부록 전용 푸터(페이지 번호 대신 라벨)
  s.addShape(pres.shapes.LINE, { x: DECK.ML, y: 7.02, w: DECK.W - DECK.ML - DECK.MR, h: 0, line: { color: COLOR.BORDER, width: 1 } });
  s.addText("반도체 장비 센서 제어 시스템", { x: DECK.ML, y: 7.06, w: 6, h: 0.3, margin: 0, fontFace: FONT.MED, fontSize: 9, color: COLOR.FAINT, valign: "middle" });
  s.addText("부록 · Q&A 대비 참고", { x: DECK.W - DECK.MR - 3, y: 7.06, w: 3, h: 0.3, margin: 0, fontFace: FONT.SEMI, fontSize: 9, color: COLOR.FAINT, align: "right", valign: "middle" });
}

s19_retro();
s20_team();
s21_future();
s22_closing();
s23_scope_appendix();

pres.writeFile({ fileName: process.env.OUT || "deck.pptx" }).then((f) => console.log("written:", f));
