export interface Address {
  k: string;
  region: string | null;
  l: string;
  m: string;
  o: string;
}

export type AddressCallback = (address: Address) => void;

export class Core {
  private URL = "https://yubinbango.github.io/yubinbango-data/data";
  private g = [
    null,
    "北海道",
    "青森県",
    "岩手県",
    "宮城県",
    "秋田県",
    "山形県",
    "福島県",
    "茨城県",
    "栃木県",
    "群馬県",
    "埼玉県",
    "千葉県",
    "東京都",
    "神奈川県",
    "新潟県",
    "富山県",
    "石川県",
    "福井県",
    "山梨県",
    "長野県",
    "岐阜県",
    "静岡県",
    "愛知県",
    "三重県",
    "滋賀県",
    "京都府",
    "大阪府",
    "兵庫県",
    "奈良県",
    "和歌山県",
    "鳥取県",
    "島根県",
    "岡山県",
    "広島県",
    "山口県",
    "徳島県",
    "香川県",
    "愛媛県",
    "高知県",
    "福岡県",
    "佐賀県",
    "長崎県",
    "熊本県",
    "大分県",
    "宮崎県",
    "鹿児島県",
    "沖縄県",
  ];
  private dataCache: { [key: string]: { [key: string]: string[] } } = {};

  constructor(postalCode = "", callback: AddressCallback) {
    if (postalCode) {
      const formattedCode = postalCode.replace(/[０-９]/g, (t) =>
        String.fromCharCode(t.charCodeAt(0) - 65248)
      );
      const digits = formattedCode.match(/\d/g) || [];
      const code = digits.join("");
      const validCode = this.validateCode(code);

      if (validCode) {
        this.fetchData(validCode, callback);
      } else {
        callback(this.emptyAddress());
      }
    }
  }

  private validateCode(code: string): string | undefined {
    return code.length === 7 ? code : undefined;
  }

  private emptyAddress(): Address {
    return { k: "", region: "", l: "", m: "", o: "" };
  }

  private parseData(data: string[]): Address {
    return data && data[0] && data[1]
      ? {
          k: data[0],
          region: this.g[parseInt(data[0])] || "",
          l: data[1],
          m: data[2],
          o: data[3],
        }
      : this.emptyAddress();
  }

  private loadScript(url: string, callback: (data: any) => void) {
    (window as any).$yubin = (data: any) => callback(data);
    const script = document.createElement("script");
    script.type = "text/javascript";
    script.charset = "UTF-8";
    script.src = url;
    document.head.appendChild(script);
  }

  private fetchData(code: string, callback: AddressCallback) {
    const prefix = code.substr(0, 3);
    if (this.dataCache[prefix] && this.dataCache[prefix][code]) {
      callback(this.parseData(this.dataCache[prefix][code]));
    } else {
      this.loadScript(`${this.URL}/${prefix}.js`, (data: any) => {
        this.dataCache[prefix] = data;
        callback(this.parseData(data[code]));
      });
    }
  }
}

// Export constants as standalone variables
export const countryKeywords = ["Japan", "JP", "JPN", "JAPAN"];
export const addressFields = [
  "p-region-id",
  "p-region",
  "p-locality",
  "p-street-address",
  "p-extended-address",
];

export class MicroformatDom {
  constructor() {
    this.initialize();
  }

  private initialize() {
    const elements = document.querySelectorAll(
      ".h-adr"
    ) as NodeListOf<HTMLElement>;
    elements.forEach((element) => {
      if (this.isJapaneseAddress(element)) {
        const postalCodes = element.querySelectorAll(
          ".p-postal-code"
        ) as NodeListOf<HTMLInputElement>;
        postalCodes[postalCodes.length - 1].addEventListener("keyup", () => {
          this.handlePostalCodeChange(this.getAddressElement(element));
        });
      }
    });
  }

  private getAddressElement(element: HTMLElement): HTMLElement {
    return element.tagName === "FORM" || element.classList.contains("h-adr")
      ? element
      : this.getAddressElement(element.parentNode as HTMLElement);
  }

  private isJapaneseAddress(element: HTMLElement): boolean {
    const country = element.querySelector(".p-country-name") as
      | HTMLElement
      | HTMLInputElement;
    const countryText =
      country instanceof HTMLInputElement ? country.value : country.innerHTML;
    return countryKeywords.includes(countryText.trim());
  }

  private handlePostalCodeChange(element: HTMLElement) {
    const postalCodeElements = element.querySelectorAll(
      ".p-postal-code"
    ) as NodeListOf<HTMLInputElement>;
    const postalCode = Array.from(postalCodeElements)
      .map((el) => el.value)
      .join("");

    new Core(postalCode, (address) => {
      this.populateAddress(element, address);
    });
  }

  private populateAddress(element: HTMLElement, address: Address) {
    const actions = [this.clearField, this.fillField];
    actions.forEach((action) => {
      addressFields.forEach((field) => {
        action(field, element, address);
      });
    });
  }

  private clearField(field: string, element: HTMLElement, address: Address) {
    const fieldElements = element.querySelectorAll(
      `.${field}`
    ) as NodeListOf<HTMLInputElement>;
    fieldElements.forEach((el) => (el.value = ""));
  }

  private fillField(field: string, element: HTMLElement, address: Address) {
    const addressMap: { [key: string]: string } = {
      "p-region-id": address.k,
      "p-region": address.region || "", // Fallback to an empty string if region is null
      "p-locality": address.l,
      "p-street-address": address.m,
      "p-extended-address": address.o,
    };
    const fieldElements = element.querySelectorAll(
      `.${field}`
    ) as NodeListOf<HTMLInputElement>;
    fieldElements.forEach((el) => (el.value += addressMap[field] || ""));
  }
}

document.addEventListener("DOMContentLoaded", () => {
  new MicroformatDom();
});
