import { ElNotification } from "element-plus";
import i18n from "@/core/plugins/i18n";

const t = i18n.global.t;

export default {
  success: (_title = t("status.success"), _message = t("status.success")) =>
    ElNotification({
      title: _title,
      message: _message,
      type: "success",
    }),

  danger: (_title = t("status.failed"), _message = t("status.failed")) =>
    ElNotification({
      title: _title,
      message: _message,
      type: "error",
    }),
};
