import { getLanguage } from "../types/LanguageTypes";
import i18n from "@/core/plugins/i18n";
import { linkEmits } from "element-plus";
const { t } = i18n.global;
import { computed } from "vue";
export const prefixLink =
  "https://mm-s3-pro.s3.ap-southeast-1.amazonaws.com/docs";

export const baDocs = {
  termAndConditions: {
    title: "termAndConditions",
    src: "Best+Execution+Policy+for+Trading+CFDs.pdf",
  },
  accountOpeningDeclarations: {
    title: "accountOpeningDeclarations",
    src: "BCR-Account-Opening-Declarations.pdf",
  },
  privacyPolicy: {
    title: "privacyPolicy",
    src: "Privacy+Policy.pdf",
  },
  publicComplaintsPolicy: {
    title: "publicComplaintsPolicy",
    src: "BCR-Public-Complaints-Policy.pdf",
  },
  targetMarketDetermination: {
    title: "targetMarketDetermination",
    src: "BCR-Target-Market-Determination.pdf",
  },
  financialServicesGuide: {
    title: "financialServicesGuide",
    src: "BCR-Financial-Services-Guide.pdf",
  },
  productDisclosureDocument: {
    title: "productDisclosureDocument",
    src: "BCR-Product-Disclosure-Statement.pdf",
  },
  contractSpecifications: {
    title: "contractSpecifications",
    src: "https://midasmkts.com/",
  },
  BestExecutionPolicyforTradingCFDs: {
    title: "BestExecutionPolicyforTradingCFDs",
    src: "Best+Execution+Policy+for+Trading+CFDs.pdf",
  },
  ClientAgreement: {
    title: "ClientAgreement",
    src: "Client+Agreement.pdf",
  },
  ComplaintHandlingPolicy: {
    title: "ComplaintHandlingPolicy",
    src: "Complaint+Handling+Policy.pdf",
  },
  ConflictOfInterestPolicy: {
    title: "ConflictOfInterestPolicy",
    src: "Conflict+of+Interest+Policy.pdf",
  },
  HedgingPolicy: {
    title: "HedgingPolicy",
    src: "Hedging+Policy.pdf",
  },
  PrivacyPolicy: {
    title: "PrivacyPolicy",
    src: "Privacy+Policy.pdf",
  },
  RiskWarningNotice: {
    title: "RiskWarningNotice",
    src: "Risk+Warning+Notice.pdf",
  },
  TermsAndConditions: {
    title: "TermsAndConditions",
    src: "../zh-tw/TERMS%26CONDITIONS.pdf",
  },
};

export const bviDocs = {
  // clientAgreement: {
  //   title: "clientAgreement",
  //   src: "BCR-Co-Pty-Ltd-Client-Agreement.pdf",
  // },
  // termsAndConditions: {
  //   title: "termsAndConditions",
  //   src: "BCR-Co-Pty-Ltd-Terms-And-Conditions.pdf",
  // },
  // accountOpeningDeclarations: {
  //   title: "accountOpeningDeclarations",
  //   src: "BCR-Co-Pty-Ltd-Account-Opening-Terms-and-Conditions.pdf",
  // },
  // orderExcutionPolicy: {
  //   title: "orderExcutionPolicy",
  //   src: "BCR-Co-Pty-Ltd-Order-Execution-Policy.pdf",
  // },
  // contractSpecifications: {
  //   title: "contractSpecifications",
  //   src: `${window.location.protocol}//${window.location.host}/`,
  // },
  // complaintsHandlingProcess: {
  //   title: "complaintsHandlingProcess",
  //   src: "BCR-Co-Pty-Ltd-Complaints-Handling-Process.pdf",
  // },
  // cookiePolicy: {
  //   title: "cookiePolicy",
  //   src: "BCR-Co-Pty-Ltd-Cookie-Policy.pdf",
  // },
  // privacyPolicy: {
  //   title: "privacyPolicy",
  //   src: "Privacy+Policy.pdf",
  // },
  // websiteTermsOfUse: {
  //   title: "websiteTermsOfUse",
  //   src: "BCR-Co-Pty-Ltd-Website-Terms-of-Use.pdf",
  // },
  // riskDisclosurePolicy: {
  //   title: "riskDisclosurePolicy",
  //   src: "BCR-Co-Pty-Ltd-Risk-Disclosure-Policy.pdf",
  // },
  // preventionOfMoneyLaundering: {
  //   title: "preventionOfMoneyLaundering",
  //   src: "BCR-Co-Pty-Ltd-Prevention-of-Money-Laundering-&-Terror-Financing-Manual.pdf",
  // },
  BestExecutionPolicyforTradingCFDs: {
    title: "BestExecutionPolicyforTradingCFDs",
    src: "Best+Execution+Policy+for+Trading+CFDs.pdf",
  },
  ClientAgreement: {
    title: "ClientAgreement",
    src: "Client+Agreement.pdf",
  },
  ComplaintHandlingPolicy: {
    title: "ComplaintHandlingPolicy",
    src: "Complaint+Handling+Policy.pdf",
  },
  ConflictOfInterestPolicy: {
    title: "ConflictOfInterestPolicy",
    src: "Conflict+of+Interest+Policy.pdf",
  },
  HedgingPolicy: {
    title: "HedgingPolicy",
    src: "Hedging+Policy.pdf",
  },
  PrivacyPolicy: {
    title: "PrivacyPolicy",
    src: "Privacy+Policy.pdf",
  },
  RiskWarningNotice: {
    title: "RiskWarningNotice",
    src: "Risk+Warning+Notice.pdf",
  },
  TermsAndConditions: {
    title: "TermsAndConditions",
    src: "TERMS%26CONDITIONS.pdf",
  },
};

export const jpDocs = {
  foreignExchangeMarginTrading: {
    title: t("type.jpAgreement.foreignExchangeMarginTrading"),
    src: "https://bcrpropublic.s3.ap-southeast-1.amazonaws.com/docs/jp/jp-jp/attention_fx_cfd.pdf",
  },
  foreignExchangeTermsAndConditions: {
    title: t("type.jpAgreement.foreignExchangeTermsAndConditions"),
    src: "https://bcrpropublic.s3.ap-southeast-1.amazonaws.com/docs/jp/jp-jp/agreement_fx.pdf",
  },

  foreginExchangeTradingManual: {
    title: t("type.jpAgreement.foreginExchangeTradingManual"),
    src: "https://bcrpropublic.s3.ap-southeast-1.amazonaws.com/docs/jp/jp-jp/fx_internet.pdf",
  },
  cfdTradingTermsAndConditions: {
    title: t("type.jpAgreement.cfdTradingTermsAndConditions"),
    src: "https://bcrpropublic.s3.ap-southeast-1.amazonaws.com/docs/jp/jp-jp/agreement_cfd_s.pdf",
  },
  cfdTradingManual: {
    title: t("type.jpAgreement.cfdTradingManual"),
    src: "https://bcrpropublic.s3.ap-southeast-1.amazonaws.com/docs/jp/jp-jp/cfd_s_internet.pdf",
  },
  productCfdTrading: {
    title: t("type.jpAgreement.productCfdTrading"),
    src: "https://bcrpropublic.s3.ap-southeast-1.amazonaws.com/docs/jp/jp-jp/attention_cfd.pdf",
  },
  commodityCfdTradingTermsAndConditions: {
    title: t("type.jpAgreement.commodityCfdTradingTermsAndConditions"),
    src: "https://bcrpropublic.s3.ap-southeast-1.amazonaws.com/docs/jp/jp-jp/agreement_cfd_p.pdf",
  },
  productCfdTradingManual: {
    title: t("type.jpAgreement.productCfdTradingManual"),
    src: "https://bcrpropublic.s3.ap-southeast-1.amazonaws.com/docs/jp/jp-jp/cfd_p_internet.pdf",
  },
  productCfdTradingForCorporations: {
    title: t("type.jpAgreement.productCfdTradingForCorporations"),
    src: "https://bcrpropublic.s3.ap-southeast-1.amazonaws.com/docs/jp/jp-jp/c_cfd_corporate.pdf",
  },
  agreementRegardingElectronicDelivery: {
    title: t("type.jpAgreement.agreementRegardingElectronicDelivery"),
    src: "https://bcrpropublic.s3.ap-southeast-1.amazonaws.com/docs/jp/jp-jp/consent.pdf",
  },
  personalInformationProtectionDeclaration: {
    title: t("type.jpAgreement.personalInformationProtectionDeclaration"),
    src: "https://bcrpropublic.s3.ap-southeast-1.amazonaws.com/docs/jp/jp-jp/kojinjyouhou_hogoseigen2.pdf",
  },
  agreementForAntiSocialForce: {
    title: t("type.jpAgreement.agreementForAntiSocialForce"),
    src: "https://bcrpropublic.s3.ap-southeast-1.amazonaws.com/docs/jp/jp-jp/hanshakaitekiseiryoku_doui.pdf",
  },
  notificationOfNotFallingUnderForeignPeps: {
    title: t("type.jpAgreement.notificationOfNotFallingUnderForeignPeps"),
    src: "https://bcrpropublic.s3.ap-southeast-1.amazonaws.com/docs/jp/jp-jp/notificationOfNotFallingUnderForeignPeps.pdf",
  },
};

export const wholeSaleDocs = {
  certificateByQualifiedAccountant: {
    title: "certificateByQualifiedAccountant",
    src: "Certificate-By-a-Qualified-Accountant.pdf",
  },
  wholesaleClientDisclosureNotice: {
    title: "wholesaleClientDisclosureNotice",
    src: "Wholesale-Client-Disclosure-Notice.pdf",
  },
  sophisticatedInvestorAcknowledgement: {
    title: "sophisticatedInvestorAcknowledgement",
    src: "Sophisticated-Investor-Acknowledgement.pdf",
  },
};

export const ibDocs = {
  referralPartyAgreement: {
    title: "referralPartyAgreement",
    src: "BCR-Co-Pty-Ltd-Referral-Party-Agreement.pdf",
  },
};

const websiteLangCode = {
  "zh-cn": "cn",
  "en-us": "en",
  "id-id": "en",
  "zh-hk": "tw",
  "zh-tw": "tw",
  "ms-my": "ms",
  "th-th": "th",
  "vi-vn": "vi",
  // "mn-mn": "mn",
  "jp-jp": "jp",
  "ko-kr": "ko",
  "km-kh": "km",
  "es-es": "es",
};

// Certificate-By-a-Qualified-Accountant.pdf
//Wholesale-Client-Disclosure-Notice.pdf
//Sophisticated+Investor+Acknowledgement.pdf

const excludeLangBa = ["id-id", "ko-kr", "km-kh"];
export const getBaDocs = (fileName: string) => {
  let lang = getLanguage.value;
  if (excludeLangBa.includes(lang)) {
    lang = "en-us";
  }
  if (fileName == "contractSpecifications") {
    const link =
      baDocs[fileName].src + `${websiteLangCode[lang]}/contract-specifications`;
    return link;
  }
  const file = baDocs[fileName].src;
  return `${prefixLink}/bvi/${lang}/${file}`;
};

const excludeLangBvi = ["id-id"];

export const getBviDocs = (fileName: string, showIbDocs = false) => {
  let lang = getLanguage.value;
  if (excludeLangBvi.includes(lang)) {
    lang = "en-us";
  }
  if (fileName == "contractSpecifications") {
    const link =
      bviDocs[fileName].src +
      `${websiteLangCode[lang]}/contract-specifications`;

    return link;
  }

  if (fileName == "referralPartyAgreement") {
    if (showIbDocs === true) {
      const file = ibDocs[fileName].src;
      let ibLang = getLanguage.value;
      if (ibLang != "zh-cn") {
        ibLang = "en-us";
      }
      return `${prefixLink}/bvi/${ibLang}/${file}`;
    } else return false;
  }

  const file = bviDocs[fileName].src;
  return `${prefixLink}/bvi/${lang}/${file}`;
};

export const getWholeSaleDocs = (fileName: string) => {
  let lang = getLanguage.value;
  if (lang != "zh-cn") {
    lang = "en-us";
  }
  const file = wholeSaleDocs[fileName].src;
  return `${prefixLink}/bvi/${lang}/${file}`;
};

export const baVerficationDocs = {
  // bcrAccountOpeningDeclarations: baDocs.accountOpeningDeclarations,
  // bcrfinancialServiceGuide: baDocs.financialServicesGuide,
  // bcrPrivacyPolicy: baDocs.privacyPolicy,
  // bcrProductDisclosureStatement: baDocs.productDisclosureDocument,
  // bcrPublicComplainPolicy: baDocs.publicComplaintsPolicy,
  // bcrTermAndConditions: baDocs.termAndConditions,
  // bcrTargetMarketDetermination: baDocs.targetMarketDetermination,
  BestExecutionPolicyforTradingCFDs: bviDocs.BestExecutionPolicyforTradingCFDs,
  ClientAgreement: bviDocs.ClientAgreement,
  ComplaintHandlingPolicy: bviDocs.ComplaintHandlingPolicy,
  ConflictOfInterestPolicy: bviDocs.ConflictOfInterestPolicy,
  HedgingPolicy: bviDocs.HedgingPolicy,
  PrivacyPolicy: bviDocs.PrivacyPolicy,
  RiskWarningNotice: bviDocs.RiskWarningNotice,
  TermsAndConditions: bviDocs.TermsAndConditions,
};

export const bviVerficationDocs = {
  BestExecutionPolicyforTradingCFDs: bviDocs.BestExecutionPolicyforTradingCFDs,
  ClientAgreement: bviDocs.ClientAgreement,
  ComplaintHandlingPolicy: bviDocs.ComplaintHandlingPolicy,
  ConflictOfInterestPolicy: bviDocs.ConflictOfInterestPolicy,
  HedgingPolicy: bviDocs.HedgingPolicy,
  PrivacyPolicy: bviDocs.PrivacyPolicy,
  RiskWarningNotice: bviDocs.RiskWarningNotice,
  TermsAndConditions: bviDocs.TermsAndConditions,

  // BCRCoPtyLtdReferralPartyAgreement: ibDocs.referralPartyAgreement,
  // BCRCoPtyLtdAccountOpeningTermsandConditions:
  //   bviDocs.accountOpeningDeclarations,
  // BCRCoPtyLtdTermsAndConditions: bviDocs.termsAndConditions,
  // BCRCoPtyLtdPreventionofMoneyLaunderingTerrorFinancingManual:
  //   bviDocs.preventionOfMoneyLaundering,
  // BCRCoPtyLtdPrivacyPolicy: bviDocs.privacyPolicy,
  // BCRCoPtyLtdRiskDisclosurePolicy: bviDocs.riskDisclosurePolicy,
  // BCRCoPtyLtdWebsiteTermsofUse: bviDocs.websiteTermsOfUse,
  // BCRCoPtyLtdOrderExecutionPolicy: bviDocs.orderExcutionPolicy,
  // BCRCoPtyLtdComplaintsHandlingProcess: bviDocs.complaintsHandlingProcess,
  // BCRCoPtyLtdCookiePolicy: bviDocs.cookiePolicy,
};
