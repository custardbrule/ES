export const generateImage = (
  canvas: HTMLCanvasElement,
  config: {
    text: string;
    fontSize: number;
    fontFamily: string;
    textColor: string;
    bgColor: string;
  }
) => {
  const ctx = canvas.getContext('2d')!;

  const { text, fontSize, fontFamily, textColor, bgColor } = config;

  // Set font to measure text
  ctx.font = `${fontSize}px ${fontFamily}`;

  // Split text into lines
  const lines = text.split('\n');
  const lineHeight = fontSize * 1.2;

  // Calculate canvas dimensions
  let maxWidth = 0;
  lines.forEach((line) => {
    const width = ctx.measureText(line).width;
    if (width > maxWidth) maxWidth = width;
  });

  const padding = 40;
  canvas.width = maxWidth + padding * 2;
  canvas.height = lines.length * lineHeight + padding * 2;

  // Fill background
  ctx.fillStyle = bgColor;
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  // Draw text
  ctx.font = `${fontSize}px ${fontFamily}`;
  ctx.fillStyle = textColor;
  ctx.textBaseline = 'top';

  lines.forEach((line, i) => {
    ctx.fillText(line, padding, padding + i * lineHeight);
  });

  return canvas.toDataURL('image/png');
};
