/**
 * BCR 文档链接配置
 * 用于注册页面的协议文档链接
 */

export const prefixLink =
  'https://mm-s3-pro.s3.ap-southeast-1.amazonaws.com/docs';

// 语言代码映射（用于文档路径）
const websiteLangCode: Record<string, string> = {
  'zh-cn': 'zh-cn',
  'zh': 'zh-cn',
  'en-us': 'en-us',
  'en': 'en-us',
  'id-id': 'en-us',
  'zh-hk': 'zh-tw',
  'zh-tw': 'zh-tw',
  'ms-my': 'ms-my',
  'th-th': 'th-th',
  'vi-vn': 'vi-vn',
  'jp-jp': 'jp-jp',
  'ko-kr': 'ko-kr',
  'km-kh': 'km-kh',
  'es-es': 'es-es',
};

// 澳洲文档配置
export const baDocs: Record<string, { title: string; src: string }> = {
  termAndConditions: {
    title: 'termAndConditions',
    src: 'Best+Execution+Policy+for+Trading+CFDs.pdf',
  },
  privacyPolicy: {
    title: 'privacyPolicy',
    src: 'Privacy+Policy.pdf',
  },
  targetMarketDetermination: {
    title: 'targetMarketDetermination',
    src: 'BCR-Target-Market-Determination.pdf',
  },
  financialServicesGuide: {
    title: 'financialServicesGuide',
    src: 'BCR-Financial-Services-Guide.pdf',
  },
  productDisclosureDocument: {
    title: 'productDisclosureDocument',
    src: 'BCR-Product-Disclosure-Statement.pdf',
  },
};

// BVI 文档配置
export const bviDocs: Record<string, { title: string; src: string }> = {
  BestExecutionPolicyforTradingCFDs: {
    title: 'BestExecutionPolicyforTradingCFDs',
    src: 'Best+Execution+Policy+for+Trading+CFDs.pdf',
  },
  ClientAgreement: {
    title: 'ClientAgreement',
    src: 'Client+Agreement.pdf',
  },
  ComplaintHandlingPolicy: {
    title: 'ComplaintHandlingPolicy',
    src: 'Complaint+Handling+Policy.pdf',
  },
  ConflictOfInterestPolicy: {
    title: 'ConflictOfInterestPolicy',
    src: 'Conflict+of+Interest+Policy.pdf',
  },
  HedgingPolicy: {
    title: 'HedgingPolicy',
    src: 'Hedging+Policy.pdf',
  },
  PrivacyPolicy: {
    title: 'PrivacyPolicy',
    src: 'Privacy+Policy.pdf',
  },
  RiskWarningNotice: {
    title: 'RiskWarningNotice',
    src: 'Risk+Warning+Notice.pdf',
  },
  TermsAndConditions: {
    title: 'TermsAndConditions',
    src: 'TERMS%26CONDITIONS.pdf',
  },
};

// BVI 验证页面使用的文档列表
export const bviVerificationDocs = {
  BestExecutionPolicyforTradingCFDs: bviDocs.BestExecutionPolicyforTradingCFDs,
  ClientAgreement: bviDocs.ClientAgreement,
  ComplaintHandlingPolicy: bviDocs.ComplaintHandlingPolicy,
  ConflictOfInterestPolicy: bviDocs.ConflictOfInterestPolicy,
  HedgingPolicy: bviDocs.HedgingPolicy,
  PrivacyPolicy: bviDocs.PrivacyPolicy,
  RiskWarningNotice: bviDocs.RiskWarningNotice,
  TermsAndConditions: bviDocs.TermsAndConditions,
};

// IB 文档配置
export const ibDocs: Record<string, { title: string; src: string }> = {
  referralPartyAgreement: {
    title: 'referralPartyAgreement',
    src: 'BCR-Co-Pty-Ltd-Referral-Party-Agreement.pdf',
  },
};

// 澳洲文档排除的语言（使用英文代替）
const excludeLangBa = ['id-id', 'ko-kr', 'km-kh'];

/**
 * 获取澳洲文档链接
 * @param fileName 文档名称（对应 baDocs 的 key）
 * @param locale 当前语言
 * @returns 文档完整链接
 */
export const getBaDocs = (fileName: string, locale: string): string => {
  // 转换 locale 格式 (zh -> zh-cn, en -> en-us)
  let lang = websiteLangCode[locale] || websiteLangCode[locale.toLowerCase()] || 'en-us';
  
  if (excludeLangBa.includes(lang)) {
    lang = 'en-us';
  }
  
  const doc = baDocs[fileName];
  if (!doc) {
    console.warn(`[bcrDocs] Document not found: ${fileName}`);
    return '';
  }
  
  return `${prefixLink}/bvi/${lang}/${doc.src}`;
};

// BVI 文档排除的语言（使用英文代替）
const excludeLangBvi = ['id-id'];

/**
 * 获取 BVI 文档链接
 * @param fileName 文档名称（对应 bviDocs 的 key）
 * @param locale 当前语言
 * @param showIbDocs 是否显示 IB 文档
 * @returns 文档完整链接，如果不应显示则返回 false
 */
export const getBviDocs = (
  fileName: string, 
  locale: string, 
  showIbDocs = false
): string | false => {
  // 转换 locale 格式 (zh -> zh-cn, en -> en-us)
  let lang = websiteLangCode[locale] || websiteLangCode[locale.toLowerCase()] || 'en-us';
  
  if (excludeLangBvi.includes(lang)) {
    lang = 'en-us';
  }

  // 处理 IB 文档
  if (fileName === 'referralPartyAgreement') {
    if (showIbDocs === true) {
      const file = ibDocs[fileName]?.src;
      if (!file) return false;
      let ibLang = lang;
      if (ibLang !== 'zh-cn') {
        ibLang = 'en-us';
      }
      return `${prefixLink}/bvi/${ibLang}/${file}`;
    }
    return false;
  }
  
  const doc = bviDocs[fileName];
  if (!doc) {
    console.warn(`[bcrDocs] Document not found: ${fileName}`);
    return '';
  }
  
  return `${prefixLink}/bvi/${lang}/${doc.src}`;
};

/**
 * 获取 BVI 验证页面的所有文档列表
 * @param locale 当前语言
 * @param showIbDocs 是否显示 IB 文档
 * @returns 文档列表 { key, title, url }[]
 */
export const getVerificationDocuments = (
  locale: string,
  showIbDocs = false
): { key: string; title: string; url: string }[] => {
  const docs: { key: string; title: string; url: string }[] = [];
  
  Object.entries(bviVerificationDocs).forEach(([key, value]) => {
    const url = getBviDocs(value.title, locale, showIbDocs);
    if (url !== false && url !== '') {
      docs.push({
        key,
        title: value.title,
        url,
      });
    }
  });

  if (showIbDocs) {
    const ibDocUrl = getBviDocs('referralPartyAgreement', locale, true);
    if (ibDocUrl !== false && ibDocUrl !== '') {
      docs.push({
        key: 'referralPartyAgreement',
        title: 'referralPartyAgreement',
        url: ibDocUrl,
      });
    }
  }
  
  return docs;
};
