import { ServiceTypes } from "@/core/types/ServiceTypes";

/**
 * 初始化表单数据
 * @param item 已保存的数据
 * @param formData 表单数据对象
 * @param defaults 默认值对象
 */
export const initializeFormData = (item: any, formData: any, defaults: any) => {
  if (Object.keys(item).length === 0) {
    Object.assign(formData, defaults);
  } else {
    Object.assign(formData, item);
  }
};

/**
 * 获取默认服务ID
 * @param hasMt5 是否有 MT5
 * @param services 服务列表
 */
export const getDefaultServiceId = (hasMt5: boolean, services: any[]) => {
  return hasMt5 ? ServiceTypes.MetaTrader5 : services[0]?.id;
};

/**
 * 获取默认货币
 * @param preferredCurrency 首选货币代码
 * @param currencies 货币列表
 */
export const getDefaultCurrency = (
  preferredCurrency: number,
  currencies: any[]
) => {
  if (currencies.some((c) => c.value === preferredCurrency)) {
    return preferredCurrency;
  }
  return currencies[0]?.value;
};

/**
 * 获取最大杠杆值
 * @param leverages 杠杆列表
 */
export const getMaxLeverage = (leverages: any[]) => {
  let max = 20;
  for (let i = 0; i < leverages.length; i++) {
    if (leverages[i].value > max) {
      max = leverages[i].value;
    }
  }
  return max;
};

/**
 * 检查是否有 MT5 服务
 * @param services 服务列表
 */
export const checkHasMt5 = (services: any[]) => {
  return services.some((item) => item.id === ServiceTypes.MetaTrader5);
};

/**
 * 创建默认问题对象
 * @param defaultValue 默认值（null 或 true）
 */
export const createDefaultQuestions = (defaultValue: boolean | null = null) => {
  return {
    q1: defaultValue,
    q2: defaultValue,
    q3: defaultValue,
    q4: defaultValue,
  };
};
