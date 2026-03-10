const extractFromElement = (element) => {
  let result = "";

  for (const child of element.childNodes) {
    if (child.nodeType === Node.TEXT_NODE && child.textContent.trim() !== "") {
      result += " " + child.textContent.trim() + " ";
      return result;
    }

    if (child.nodeType === Node.ELEMENT_NODE) {
      result += extractFromElement(child);
    }
  }
  return result;
};

export const extractText = (htmlString: string) => {
  const parser = new DOMParser();
  const doc = parser.parseFromString(htmlString, "text/html");
  return extractFromElement(doc.body).trim();
};
