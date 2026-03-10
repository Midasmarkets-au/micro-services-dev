

export interface ILanguage {
  name: string;
  code: string;
  flag: string;
  englishName?: string;
}

export class LanguageCodes {
  public static enUS = "en-us";
  public static zhCN = "zh-cn";
  public static zhHK = "zh-hk";
  public static zhTW = "zh-tw";
  public static viVN = "vi-vn";
  public static thTh = "th-th";
  public static jpJP = "jp-jp";
  // public static mnMN = "mn-mn";
  public static idID = "id-id";
  public static msMY = "ms-my";
  public static koKR = "ko-kr";
  public static kmKH = "km-kh";
  public static esES = "es-es";

  public static all = [
    this.enUS,
    this.zhCN,
    this.zhHK,
    this.viVN,
    this.thTh,
    this.zhTW,
    this.jpJP,
    // this.mnMN,
    this.idID,
    this.msMY,
    this.koKR,
    this.kmKH,
    this.esES,
  ];
  public static activated = [
    this.enUS,
    this.zhCN,
    this.zhTW,
    this.viVN,
    this.thTh,
    this.jpJP,
    // this.mnMN,
    this.idID,
    this.msMY,
    this.koKR,
    this.kmKH,
    this.esES,
  ];
}
// 'en-us' => 'en',
// 'zh-cn' => 'zh',
export class LanguageTypes {
  public static enUS: ILanguage = {
    name: "English",
    code: "en-us",
    englishName: "English",
    flag: "/images/flags/us.svg",
  } as ILanguage;

  public static zhCN: ILanguage = {
    name: "简体中文",
    code: "zh-cn",
    englishName: "Simplified Chinese",
    flag: "/images/flags/cn.svg",
  } as ILanguage;

  public static zhHK: ILanguage = {
    name: "繁体中文",
    code: "zh-hk",
    englishName: "Traditional Chinese",
    flag: "/images/flags/hong-kong.svg",
  } as ILanguage;

  public static zhTW: ILanguage = {
    name: "繁体中文",
    code: "zh-tw",
    englishName: "Traditional Chinese",
    flag: "/images/flags/tw.svg",
  } as ILanguage;

  public static viVN: ILanguage = {
    name: "Tiếng Việt Nam",
    code: "vi-vn",
    englishName: "Vietnamese",
    flag: "/images/flags/vn.svg",
  } as ILanguage;

  public static thTh: ILanguage = {
    name: "ภาษาไทย",
    code: "th-th",
    englishName: "Thai",
    flag: "/images/flags/th.svg",
  } as ILanguage;

  public static jpJP: ILanguage = {
    name: "日本語",
    code: "jp-jp",
    englishName: "Japanese",
    flag: "/images/flags/jp.svg",
  } as ILanguage;

  // public static mnMN: ILanguage = {
  //   name: "Монгол Хэл",
  //   code: "mn-mn",
  //   englishName: "Mongolian",
  //   flag: "/images/flags/mn.svg",
  // } as ILanguage;

  public static idID: ILanguage = {
    name: "Bahasa Indonesia",
    code: "id-id",
    englishName: "Indonesian",
    flag: "/images/flags/id.svg",
  } as ILanguage;

  public static msMY: ILanguage = {
    name: "Bahasa Melayu",
    code: "ms-my",
    englishName: "Malay",
    flag: "/images/flags/my.svg",
  } as ILanguage;

  public static koKR: ILanguage = {
    name: "한국어",
    code: "ko-kr",
    englishName: "Korean",
    flag: "/images/flags/kr.svg",
  } as ILanguage;

  public static kmKH: ILanguage = {
    name: "ភាសាខ្មែរ",
    code: "km-kh",
    englishName: "Khmer",
    flag: "/images/flags/kh.svg",
  } as ILanguage;

  public static esES: ILanguage = {
    name: "Español",
    code: "es-es",
    englishName: "Spanish",
    flag: "/images/flags/es.svg",
  } as ILanguage;

  public static all: ILanguage[] = [
    this.enUS,
    this.zhCN,
    this.zhTW,
    this.viVN,
    this.thTh,
    this.jpJP,
    // this.mnMN,
    this.idID,
    this.msMY,
    this.koKR,
    this.kmKH,
    this.esES,
  ];
}

 
