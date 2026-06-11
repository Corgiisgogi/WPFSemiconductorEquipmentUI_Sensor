// 프로토타입: 단일 슬라이드(위험 엔진, 2단 레이아웃)로 레이아웃 시스템을 검증한다.
const pptxgen = require("pptxgenjs");
const { DECK, COLOR, FONT, shadow, sideBar, header, footer, screenshotCard } = require("./theme");

const SHOT = "../스크린샷 2026-06-11 093028.png"; // 자동정지(DANGER) 화면
const SHOT_RATIO = 1426 / 893;

const pres = new pptxgen();
pres.defineLayout({ name: "WIDE", width: DECK.W, height: DECK.H });
pres.layout = "WIDE";
pres.author = "팀(placeholder)";
pres.title = "반도체 장비 센서 제어 시스템";

const slide = pres.addSlide();
slide.background = { color: COLOR.BG };
sideBar(pres, slide);
header(pres, slide, { kicker: "Part 4 · 핵심 기능 구현", title: "위험 엔진 (Risk Engine)" });

// ── 좌측 컬럼 ───────────────────────────────────────────────
const LX = DECK.ML;
// 자동 줄바꿈에 맡기지 않고 자연스러운 절 경계(콤마 뒤)에서 수동 줄바꿈한다.
slide.addText([
  { text: "센서값을 임계값과 비교해 위험을 누적·판정하고,", options: { breakLine: true } },
  { text: "한도 도달 시 공정을 자동 종료합니다." },
], {
  x: LX, y: 1.55, w: 5.6, h: 0.6, margin: 0,
  fontFace: FONT.MED, fontSize: 14, color: COLOR.SUB, align: "left", valign: "top", lineSpacingMultiple: 1.1,
});

const rows = [
  ["임계 평가", "압력·진동·온도·습도를 설정 임계값과 비교"],
  ["슬라이딩 윈도우 누적", "RiskWindowSeconds(기본 60초) 내 위험 이벤트를 누적"],
  ["자동 종료", "누적이 한도(기본 2회) 도달 시 공정 정지 + 경고 램프"],
  ["노이즈 방지", "경고 해제 디바운스 / 값 정지(stale) 시 누적 보류"],
];
const ROW_Y = 2.35, ROW_H = 0.92;
rows.forEach(([term, desc], i) => {
  const y = ROW_Y + i * ROW_H;
  slide.addShape(pres.shapes.ROUNDED_RECTANGLE, {
    x: LX, y, w: 0.34, h: 0.34, rectRadius: 0.06, fill: { color: COLOR.PRIMARY }, line: { type: "none" },
  });
  slide.addText(String(i + 1), {
    x: LX, y, w: 0.34, h: 0.34, margin: 0,
    fontFace: FONT.BOLD, fontSize: 13, color: "FFFFFF", align: "center", valign: "middle",
  });
  slide.addText(term, {
    x: LX + 0.5, y: y - 0.04, w: 4.9, h: 0.32, margin: 0,
    fontFace: FONT.SEMI, fontSize: 15, color: COLOR.INK, align: "left", valign: "middle",
  });
  slide.addText(desc, {
    x: LX + 0.5, y: y + 0.3, w: 4.95, h: 0.4, margin: 0,
    fontFace: FONT.REG, fontSize: 12.5, color: COLOR.SUB, align: "left", valign: "top", lineSpacingMultiple: 1.05,
  });
});

// ── 우측 컬럼: 스크린샷 + 캡션 ──────────────────────────────
// 좌표는 사용자가 직접 맞춘 proto 수정본을 그대로 반영(스크린샷을 크게, 위로; 캡션을 카드 폭에 맞춤).
const CARD_X = 5.736, CARD_Y = 0.982, CARD_W = 6.963;
screenshotCard(pres, slide, { path: SHOT, x: CARD_X, y: CARD_Y, w: CARD_W, imgRatio: SHOT_RATIO, inset: 0.108 });

const CAP_Y = 5.598, CAP_H = 0.62;
slide.addShape(pres.shapes.RECTANGLE, {
  x: 5.801, y: CAP_Y, w: 6.899, h: CAP_H, fill: { color: COLOR.PANEL }, line: { type: "none" },
});
slide.addShape(pres.shapes.RECTANGLE, {
  x: CARD_X, y: CAP_Y, w: 0.07, h: CAP_H, fill: { color: COLOR.RED }, line: { type: "none" },
});
slide.addText(
  [
    { text: "진동 9.5 기준 초과 · 위험 카운트 2/2", options: { fontFace: FONT.SEMI, color: COLOR.INK } },
    { text: "  →  공정 자동 정지", options: { fontFace: FONT.SEMI, color: COLOR.RED } },
  ],
  { x: 6.129, y: CAP_Y, w: 5.75, h: CAP_H, margin: 0, fontSize: 13, align: "left", valign: "middle" }
);

footer(pres, slide, { page: 12, total: 22 });

pres.writeFile({ fileName: "proto.pptx" }).then((f) => console.log("written:", f));
