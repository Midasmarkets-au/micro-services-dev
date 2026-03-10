import { InjectionKey } from "vue/dist/vue";
import {
  HandleShowAddCommentType,
  HandleShowConfirmBoxType,
} from "@/core/types/TenantGlobalInjectionKeys";

export default {
  OPEN_FILE_MODAL: Symbol("OPEN_FILE_MODAL"),
  OPEN_CONFIRM_MODAL: Symbol(
    "CONFIRM_MODAL"
  ) as InjectionKey<HandleShowConfirmBoxType>,
  OPEN_USER_DETAILS: Symbol("OPEN_USER_DETAILS"),
  FILE_SHOW_REF: Symbol("FILE_SHOW_REF"),
  OPEN_REJECT_REASON_MODAL: Symbol("OPEN_REJECT_REASON_MODAL"),
  OPEN_ADD_COMMENT_MODAL: Symbol(
    "OPEN_ADD_COMMENT_MODAL"
  ) as InjectionKey<HandleShowAddCommentType>,
  EVENT_SHOP_DETAIL: Symbol("EVENT_SHOP_DETAIL"),
  EVENT_SHOP_USER_DETAIL: Symbol("EVENT_SHOP_USER_DETAIL"),
};
