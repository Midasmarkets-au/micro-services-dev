import { useStore } from "@/store";
import { computed } from "vue";
import { LanguageTypes } from "@/core/types/LanguageTypes";

export enum TenantTypes {
  au = 1,
  bvi = 10000,
  // mn = 10002,
  sea = 10004,
  jp = 10005,
}

export const TenantOptions = [
  { label: "VU", value: TenantTypes.bvi },
  { label: "SEA", value: TenantTypes.sea },
  { label: "Australia", value: TenantTypes.au },
  { label: "Japan", value: TenantTypes.jp },
  // {label: 'Mongolia', value: TenantTypes.mn},
];

export enum tenancies {
  au = "au",
  bvi = "bvi",
  sea = "sea",
  jp = "jp",
}

export const websiteTenant = [
  { label: "VU", value: "bvi" },
  { label: "SEA", value: "vn" },
  { label: "AU", value: "ba" },
  { label: "CN", value: "cn" },
  { label: "TW", value: "tw" },
];

export const getTenancy = computed(() => {
  // const jpSites = ["jp.thebcr.com", "portal.isec.jp", "thebcrjp.com"];
  const jpSites = ["thebcrjp.com"];
  const hostname = window.location.hostname;

  if (jpSites.includes(hostname)) {
    return tenancies.jp;
  }
  /**
   * 由于这里是获取的环境变量，因线上环境只有一套，环境变量只有一个，
   * 目前无法确认AU的site是否有单独部署，且代码中有使用getTenancy相关逻辑：
   * 1.当site为au时，会取前端环境变量中的site跟 auth/c的变量进行对比，如果相等，需要设置tenantId
   * 2.二次验证的邮件 会取前端环境变量中的site来设置发邮件人信息
   */
  let site = process.env.VUE_APP_SITE;
  const type = hostname.split(".")[0];
  if (tenancies[type]) {
    site = type;
  }
  if (tenancies[site] === undefined) {
    site = tenancies.bvi;
  }
  return site;
});

const getTenancyByLocalStorage = () => {
  const user = localStorage.getItem("user");
  if (user) {
    console.log(JSON.parse(user)["tenancy"]);
    return JSON.parse(user)["tenancy"];
  }
  console.log("No user in local storage");
  return null;
};

export const jpSites = ["thebcrjp.com"];

export const getVerificationTenancy = () => {
  const hostname = window.location.hostname;

  if (jpSites.includes(hostname)) {
    return tenancies.jp;
  }

  const userTenancy = getUserTenancy();
  if (userTenancy && userTenancy != tenancies.jp) {
    return userTenancy;
  }

  let site = process.env.VUE_APP_SITE;
  if (tenancies[site] === undefined) {
    site = tenancies.bvi;
  }
  return site;
};

export const getUserTenancy = () => {
  const store = useStore();
  const user = computed(() => store.state.AuthModule?.user);
  return user?.value.tenancy;
};

export enum tenantNames {
  bvi = "MDM",
  jp = "I Securities ",
}

export const getTenantName = computed(() => {
  const tenancy = getTenancy.value;
  return tenantNames[tenancy];
});

export const canLiveChat = computed(() => {
  const tenancy = getTenancy.value;
  const canLiveChatList = [tenancies.bvi];
  return canLiveChatList.includes(tenancy);
});

export const getTenantFavicon = computed(() => {
  const tenancy = getTenancy.value;
  const tenantFavicon = {
    [tenancies.bvi]: "/favicon.ico",
    [tenancies.jp]: "/isec-favicon.ico",
  };
  return tenantFavicon[tenancy];
});

export const setFavicon = () => {
  let link: HTMLLinkElement | null =
    document.querySelector("link[rel~='icon']");
  if (!link) {
    link = document.createElement("link");
    link.rel = "icon";
    document.head.appendChild(link);
  }
  link.href = getTenantFavicon.value;
};

export const getTenantLogo = computed(() => {
  const tenancy = getTenancy.value;
  const tenantLogo = {
    [tenancies.bvi]: {
      src: "/images/logos/logo@2x.png",
      style: {
        width: "33px",
      },
    },
    [tenancies.au]: {
      src: "/images/logos/logo@2x.png",
      style: {
        width: "33px",
      },
    },
    [tenancies.sea]: {
      src: "/images/logos/logo@2x.png",
      style: {
        width: "33px",
      },
    },
    [tenancies.jp]: {
      src: "/images/logos/isec-logo.png",
      style: {
        width: "33px",
      },
    },
  };
  if (tenantLogo[tenancy] == undefined) {
    return {
      src: "/images/logos/logo.svg",
      style: {
        width: "33px",
      },
    };
  }
  return tenantLogo[tenancy];
});

export const getBackendTenantLogo = computed(() => {
  const tenancy = getTenancy.value;
  const tenantLogo = {
    [tenancies.bvi]: {
      src: "/images/logos/logo@2x.png",
      class: "h-40px app-sidebar-logo-default",
    },
    [tenancies.jp]: {
      src: "/images/logos/isec-logo.png",
      class: "h-30px app-sidebar-logo-default",
    },
  };
  return tenantLogo[tenancy];
});

export const getBackendMiniLogo = computed(() => {
  const tenancy = getTenancy.value;
  const tenantLogo = {
    [tenancies.bvi]: {
      src: "/images/logos/default-small.svg",
      class: "h-20px app-sidebar-logo-minimize",
    },
    [tenancies.jp]: {
      src: "/images/logos/jp-logo-sm.jpg",
      class: "h-30px app-sidebar-logo-minimize",
    },
  };
  return tenantLogo[tenancy];
});

export const getTenantWalletLogo = computed(() => {
  const tenancy = getTenancy.value;
  const tenantWalletLogo = {
    [tenancies.bvi]: {
      src: "/images/logos/logo-wallet-bg.png",
      style: {},
    },
    [tenancies.jp]: {
      src: "/images/logos/isec-logo.png",
      style: {
        width: "100px",
        height: "auto",
      },
    },
  };
  return tenantWalletLogo[tenancy];
});

export const getTenantLanguagesOptions = computed(() => {
  const tenancy = getTenancy.value;
  const tenantLanguagesOptions = {
    [tenancies.bvi]: LanguageTypes.all,
    [tenancies.au]: LanguageTypes.all,
    [tenancies.jp]: [
      LanguageTypes.enUS,
      LanguageTypes.jpJP,
      LanguageTypes.zhCN,
      LanguageTypes.zhTW,
    ],
    [tenancies.sea]: LanguageTypes.all,
  };
  return tenantLanguagesOptions[tenancy];
});

export const getBackendFooterName = computed(() => {
  const tenancy = getTenancy.value;
  switch (tenancy) {
    case tenancies.jp:
      return "ISec";
    default:
      return "MM Pro";
  }
});
