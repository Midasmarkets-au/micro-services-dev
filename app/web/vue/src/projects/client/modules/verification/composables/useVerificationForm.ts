import { inject } from "vue";

/**
 * 验证表单通用 Composable
 * 用于获取所有验证组件共享的 inject 数据
 */
export const useVerificationForm = () => {
  const items = inject<any>("items");
  const formData = inject<any>("formData");
  const isSubmitting = inject<any>("isSubmitting");
  const user = inject<any>("user");
  const accountTypeSelections = inject<any>("accountTypeSelections");

  return {
    items,
    formData,
    isSubmitting,
    user,
    accountTypeSelections,
  };
};
