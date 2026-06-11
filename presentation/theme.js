// 발표 덱 공용 테마 + 레이아웃 헬퍼 (Pretendard 전용, 모던)
// 모든 슬라이드가 재사용하는 색·폰트·치수·크롬(사이드바/헤더/푸터/스크린샷 카드).

const DECK = {
  W: 13.333,
  H: 7.5,
  ML: 0.75,   // 콘텐츠 좌측 시작 (사이드바 0.1 이후 여백 포함)
  MR: 0.63,   // 우측 여백
};

const COLOR = {
  BG: "FFFFFF",
  INK: "1A1A1A",
  SUB: "6B7280",
  FAINT: "9AA1AC",
  PANEL: "F4F6F8",
  BORDER: "E2E5EA",
  PRIMARY: "155FB0",
  PRIMARY_DK: "0F4C8C",
  GREEN: "2E9E6B",
  AMBER: "E5A400",
  RED: "D64545",
};

// Pretendard 9종. 강조는 굵기/색으로만(이탤릭 없음).
const FONT = {
  BLACK: "Pretendard Black",
  XBOLD: "Pretendard ExtraBold",
  BOLD: "Pretendard SemiBold", // 제목용 굵기
  SEMI: "Pretendard SemiBold",
  MED: "Pretendard Medium",
  REG: "Pretendard",
  LIGHT: "Pretendard Light",
};

const shadow = (o = {}) => ({
  type: "outer", color: "000000", blur: 9, offset: 3, angle: 135, opacity: 0.12, ...o,
});

// 좌측 전체 높이 블루 액센트 띠 (반복 모티프)
function sideBar(pres, slide) {
  slide.addShape(pres.shapes.RECTANGLE, {
    x: 0, y: 0, w: 0.16, h: DECK.H, fill: { color: COLOR.PRIMARY }, line: { type: "none" },
  });
}

// 헤더: 키커(파트 라벨) + 제목. 제목 밑줄 사용 안 함(여백·굵기 대비로 위계).
function header(pres, slide, { kicker, title }) {
  slide.addText(kicker.toUpperCase(), {
    x: DECK.ML, y: 0.5, w: 10, h: 0.28, margin: 0,
    fontFace: FONT.SEMI, fontSize: 11.5, color: COLOR.PRIMARY, charSpacing: 2, align: "left", valign: "middle",
  });
  slide.addText(title, {
    x: DECK.ML - 0.02, y: 0.8, w: 11.8, h: 0.66, margin: 0,
    fontFace: FONT.BOLD, fontSize: 28, color: COLOR.INK, align: "left", valign: "middle",
  });
}

// 푸터: 얇은 구분선 + 좌 덱명 / 우 페이지
function footer(pres, slide, { page, total }) {
  slide.addShape(pres.shapes.LINE, {
    x: DECK.ML, y: 7.02, w: DECK.W - DECK.ML - DECK.MR, h: 0, line: { color: COLOR.BORDER, width: 1 },
  });
  slide.addText("반도체 장비 센서 제어 시스템", {
    x: DECK.ML, y: 7.06, w: 6, h: 0.3, margin: 0,
    fontFace: FONT.MED, fontSize: 9, color: COLOR.FAINT, align: "left", valign: "middle",
  });
  slide.addText(`${page} / ${total}`, {
    x: DECK.W - DECK.MR - 1.5, y: 7.06, w: 1.5, h: 0.3, margin: 0,
    fontFace: FONT.SEMI, fontSize: 9, color: COLOR.FAINT, align: "right", valign: "middle",
  });
}

// 스크린샷 카드: 흰 카드 + 옅은 보더 + 그림자, 내부에 이미지 인셋 배치
function screenshotCard(pres, slide, { path, x, y, w, imgRatio, inset = 0.108 }) {
  const imgW = w - inset * 2;
  const imgH = imgW / imgRatio;
  const cardH = imgH + inset * 2;
  slide.addShape(pres.shapes.RECTANGLE, {
    x, y, w, h: cardH, fill: { color: COLOR.BG }, line: { color: COLOR.BORDER, width: 1 }, shadow: shadow(),
  });
  slide.addImage({ path, x: x + inset, y: y + inset, w: imgW, h: imgH });
  return cardH;
}

// 2단 스크린샷 슬라이드의 우측 카드 표준 좌표(사용자 튜닝값). 앱 창 비율은 1426/893.
const RIGHT_CARD = { x: 5.736, y: 0.982, w: 6.963, inset: 0.108 };
const SHOT_RATIO = 1426 / 893;

// 캡션 바: 카드 좌측 끝에 색 액센트 바 + 회색 패널 + 텍스트(runs)
function captionBar(pres, slide, { x, y, w, h = 0.62, accent = COLOR.RED, runs }) {
  slide.addShape(pres.shapes.RECTANGLE, {
    x: x + 0.065, y, w: w - 0.065, h, fill: { color: COLOR.PANEL }, line: { type: "none" },
  });
  slide.addShape(pres.shapes.RECTANGLE, {
    x, y, w: 0.07, h, fill: { color: accent }, line: { type: "none" },
  });
  slide.addText(runs, { x: x + 0.393, y, w: w - 0.55, h, margin: 0, fontSize: 13, align: "left", valign: "middle" });
}

// 번호 배지 + 용어 + 설명 행 묶음 (좌측 컬럼 표준)
function numberedRows(pres, slide, { x, startY, rowH = 0.92, items, descW = 4.95 }) {
  items.forEach(([term, desc], i) => {
    const y = startY + i * rowH;
    slide.addShape(pres.shapes.ROUNDED_RECTANGLE, {
      x, y, w: 0.34, h: 0.34, rectRadius: 0.06, fill: { color: COLOR.PRIMARY }, line: { type: "none" },
    });
    slide.addText(String(i + 1), {
      x, y, w: 0.34, h: 0.34, margin: 0, fontFace: FONT.BOLD, fontSize: 13, color: "FFFFFF", align: "center", valign: "middle",
    });
    slide.addText(term, {
      x: x + 0.5, y: y - 0.04, w: descW, h: 0.32, margin: 0, fontFace: FONT.SEMI, fontSize: 15, color: COLOR.INK, align: "left", valign: "middle",
    });
    slide.addText(desc, {
      x: x + 0.5, y: y + 0.3, w: descW, h: 0.42, margin: 0, fontFace: FONT.REG, fontSize: 12.5, color: COLOR.SUB, align: "left", valign: "top", lineSpacingMultiple: 1.05,
    });
  });
}

// 좌측 색 액센트 바가 있는 카드(패널 + 굵은 제목 + 설명). captionBar와 같은 모티프.
function accentCard(pres, slide, { x, y, w, h, accent = COLOR.PRIMARY, kicker, title, desc }) {
  slide.addShape(pres.shapes.RECTANGLE, {
    x, y, w, h, fill: { color: COLOR.PANEL }, line: { type: "none" }, shadow: shadow({ blur: 6, offset: 2, opacity: 0.08 }),
  });
  slide.addShape(pres.shapes.RECTANGLE, {
    x, y, w: 0.08, h, fill: { color: accent }, line: { type: "none" },
  });
  const ix = x + 0.34;
  let ty = y + 0.3;
  if (kicker) {
    slide.addText(kicker.toUpperCase(), {
      x: ix, y: ty, w: w - 0.6, h: 0.24, margin: 0,
      fontFace: FONT.SEMI, fontSize: 10, color: accent, charSpacing: 1.5, valign: "middle",
    });
    ty += 0.3;
  }
  slide.addText(title, {
    x: ix, y: ty, w: w - 0.6, h: 0.38, margin: 0,
    fontFace: FONT.BOLD, fontSize: 16, color: COLOR.INK, valign: "middle",
  });
  if (desc) {
    slide.addText(desc, {
      x: ix, y: ty + 0.44, w: w - 0.62, h: h - (ty - y) - 0.5, margin: 0,
      fontFace: FONT.REG, fontSize: 12.5, color: COLOR.SUB, align: "left", valign: "top", lineSpacingMultiple: 1.18,
    });
  }
}

// 체크 행: 작은 색 점 + 굵은 용어 + 회색 설명(한 줄). 요구사항/목록 패널 내부용.
function checkRow(pres, slide, { x, y, w, accent = COLOR.PRIMARY, term, desc }) {
  slide.addShape(pres.shapes.OVAL, {
    x, y: y + 0.07, w: 0.13, h: 0.13, fill: { color: accent }, line: { type: "none" },
  });
  slide.addText([
    { text: term, options: { fontFace: FONT.SEMI, color: COLOR.INK } },
    { text: "   " + desc, options: { fontFace: FONT.REG, color: COLOR.SUB } },
  ], {
    x: x + 0.28, y: y - 0.04, w: w - 0.28, h: 0.36, margin: 0,
    fontSize: 12.5, align: "left", valign: "middle",
  });
}

// 작은 칩(태그). fill 지정 시 배경색 변경(예: 회색 밴드 위에서는 흰색으로 띄움).
function chip(pres, slide, { x, y, w, text, fill = COLOR.PANEL, border }) {
  slide.addShape(pres.shapes.ROUNDED_RECTANGLE, {
    x, y, w, h: 0.42, rectRadius: 0.08, fill: { color: fill },
    line: border ? { color: border, width: 1 } : { type: "none" },
  });
  slide.addText(text, {
    x, y, w, h: 0.42, margin: 0, fontFace: FONT.SEMI, fontSize: 11.5, color: COLOR.PRIMARY_DK, align: "center", valign: "middle",
  });
}

module.exports = {
  DECK, COLOR, FONT, RIGHT_CARD, SHOT_RATIO, shadow,
  sideBar, header, footer, screenshotCard, captionBar, numberedRows, chip,
  accentCard, checkRow,
};
