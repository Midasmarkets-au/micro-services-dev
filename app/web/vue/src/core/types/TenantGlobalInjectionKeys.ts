import type { InjectionKey } from "vue";
import { WSSignalR } from "@/core/plugins/signalr";

export type ShowConfirmBoxOptionsType = {
  confirmTitle?: string;
  confirmText?: string;
  confirmColor?: string;
  cancelTitle?: string;
  cancelText?: string;
  cancelColor?: string;
  disableFooter?: boolean;
};

export type HandleShowConfirmBoxType = (
  handleConfirm?: () => void,
  handleCancel?: () => void,
  options?: ShowConfirmBoxOptionsType
) => void;

export type HandleShowAddCommentType = (
  handleSubmitComment?: (cmt: string) => void,
  handleCancel?: () => void,
  options?: ShowConfirmBoxOptionsType
) => void;

export default {
  OPEN_FILE_MODAL: Symbol("OPEN_FILE_MODAL"),
  OPEN_CONFIRM_MODAL: Symbol(
    "CONFIRM_MODAL"
  ) as InjectionKey<HandleShowConfirmBoxType>,
  OPEN_USER_DETAILS: Symbol("OPEN_USER_DETAILS"),
  OPEN_COMMENT_VIEW: Symbol("OPEN_COMMENT_VIEW"),
  FILE_SHOW_REF: Symbol("FILE_SHOW_REF"),
  OPEN_REJECT_REASON_MODAL: Symbol(
    "OPEN_REJECT_REASON_MODAL"
  ) as InjectionKey<any>,
  OPEN_ADD_COMMENT_MODAL: Symbol(
    "OPEN_ADD_COMMENT_MODAL"
  ) as InjectionKey<HandleShowAddCommentType>,
  WS_SIGNAL_R: Symbol("WS_SIGNAL_R") as InjectionKey<WSSignalR>,
};
