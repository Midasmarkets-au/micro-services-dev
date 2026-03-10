import Swal from "sweetalert2/dist/sweetalert2.js";
import i18n from "@/core/plugins/i18n";
import { processErrorMessage } from "@/core/types/ErrorMessage";
const t = i18n.global.t;

export default {
  success: (
    _text = t("tip.submitSuccess"),
    _title = "",
    _confirmButtonText = "OK",
    _confirmButton = "btn fw-semobold btn btn-sm btn-radius btn-primary"
  ) =>
    Swal.fire({
      title: _title,
      text: _text,
      icon: "success",
      buttonsStyling: false,
      confirmButtonText: _confirmButtonText,
      customClass: {
        confirmButton: _confirmButton,
        container: "my-swal-overlay",
      },
    }),

  info: (
    _text = t("title.info"),
    _title = "Title",
    _confirmButtonText = "OK",
    _confirmButton = "btn btn-sm btn-radius fw-semobold btn-light-info"
  ) =>
    Swal.fire({
      title: _title,
      text: _text,
      icon: "info",
      buttonsStyling: false,
      showCloseButton: true,
      confirmButtonText: _confirmButtonText,
      customClass: {
        confirmButton: _confirmButton,
        container: "my-swal-overlay",
      },
    }),

  warning: (
    _text = t("tip.pleaseTryAgain"),
    _title = "",
    _confirmButtonText = "OK",
    _confirmButton = "btn fw-semobold btn-sm btn-radius btn-light-warning"
  ) =>
    Swal.fire({
      title: _title,
      text: _text,
      icon: "warning",
      buttonsStyling: false,
      confirmButtonText: _confirmButtonText,
      customClass: {
        confirmButton: _confirmButton,
        container: "my-swal-overlay",
      },
    }),

  error: (
    _text,
    _title = "",
    _confirmButtonText = "OK",
    _confirmButton = "btn fw-semobold btn-sm btn-radius btn-light-danger"
  ) => {
    // let statusCode = "";
    _text = processErrorMessage(_text);
    return Swal.fire({
      title: _title,
      text: _text,
      icon: "error",
      buttonsStyling: false,
      confirmButtonText: _confirmButtonText,
      customClass: {
        confirmButton: _confirmButton,
        container: "my-swal-overlay",
      },
    });
  },

  errorMsgOnly: (
    _text,
    _title = "",
    _confirmButtonText = "OK",
    _confirmButton = "btn fw-semobold btn-sm btn-radius btn-light-danger"
  ) => {
    // let statusCode = "";

    return Swal.fire({
      title: _title,
      text: _text,
      icon: "error",
      buttonsStyling: false,
      confirmButtonText: _confirmButtonText,
      customClass: {
        confirmButton: _confirmButton,
        container: "my-swal-overlay",
      },
    });
  },
};
