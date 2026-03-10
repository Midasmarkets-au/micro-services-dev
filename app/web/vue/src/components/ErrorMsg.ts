import { App } from "vue";
import Swal from "sweetalert2/dist/sweetalert2.min.js";
import i18n from "@/core/plugins/i18n";

class ErrorMsg {
  /**
   * @description property to share vue instance
   */
  public static vueInstance: App;

  /**
   * @description initialize vue axios
   */
  public static init(app: App<Element>) {
    ErrorMsg.vueInstance = app;
    ErrorMsg.vueInstance.use(Swal);
  }

  public static show(response) {
    if (response == undefined) {
      Swal.fire({
        text: i18n.global.t("error.unknown"),
        icon: "warning",
        buttonsStyling: false,
        confirmButtonText: i18n.global.t("actionclose"),
        customClass: {
          confirmButton: "btn fw-semobold btn-light-danger",
        },
      });
    } else if (response.status == 404) {
      Swal.fire({
        text: i18n.global.t("error.404"),
        icon: "warning",
        buttonsStyling: false,
        confirmButtonText: i18n.global.t("actionclose"),
        customClass: {
          confirmButton: "btn fw-semobold btn-light-danger",
        },
      });
    } else if (response.status == 403) {
      Swal.fire({
        text: i18n.global.t("error.403"),
        icon: "warning",
        buttonsStyling: false,
        confirmButtonText: i18n.global.t("actionclose"),
        customClass: {
          confirmButton: "btn fw-semobold btn-light-danger",
        },
      });
    } else {
      Swal.fire({
        text: i18n.global.t("error.unknown"),
        icon: "warning",
        buttonsStyling: false,
        confirmButtonText: i18n.global.t("actionclose") + response.status,
        customClass: {
          confirmButton: "btn fw-semobold btn-light-danger",
        },
      });
    }
  }
}

export default ErrorMsg;
