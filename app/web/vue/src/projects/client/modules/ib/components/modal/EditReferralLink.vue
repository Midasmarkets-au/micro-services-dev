<template>
  <div
    class="modal fade ibLinkDetailBackdrop"
    id="kt_modal_iblibk_detail"
    tabindex="-1"
    aria-hidden="true"
    ref="LinkEditModalRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-650px">
      <div class="modal-content rounded-3">
        <div class="modal-header" id="kt_modal_new_address_header">
          <div class="d-flex align-items-center">
            <h2 class="fs-2">{{ $t("action.editLink") }}</h2>
            <div v-if="item.serviceType == AccountRoleTypes.Client">
              <el-switch
                v-model="isDefault"
                inline-prompt
                active-text="Default"
                inactive-text="Not Default"
                :active-value="1"
                :inactive-value="0"
                size="large"
                class="ms-4"
                :loading="isUpdating"
                @change="setDefaultLink"
                style="
                  --el-switch-on-color: #000f32;
                  --el-switch-off-color: #0a46aa;
                "
              />
            </div>
          </div>
          <div data-bs-dismiss="modal">
            <span>
              <inline-svg src="/images/icons/arrows/arr061.svg" />
            </span>
          </div>
        </div>
        <!-- ------------------------------------------------------------------ -->

        <div class="form-outer">
          <div class="card-body py-0 mt-4 mx-6">
            <div class="row mb-5"></div>
            <div class="row">
              <div :class="isMobile ? 'col-12' : 'col-12'">
                <div class="d-flex align-items-center">
                  <div class="dot me-3"></div>
                  <span class="stepContent">{{ $t("tip.linkName") }}</span>
                </div>

                <Field
                  class="form-control form-control-solid mt-5"
                  :placeholder="$t('tip.pleaseInput')"
                  name="linkName"
                  v-model="item.name"
                  disabled
                />
              </div>
              <div :class="isMobile ? 'col-12 mt-6' : 'col-12'">
                <div class="d-flex align-items-center mt-5">
                  <div class="dot me-3"></div>
                  <span class="stepContent">{{ $t("tip.newLinkName") }}</span>
                </div>

                <Field
                  class="form-control form-control-solid mt-5"
                  :placeholder="$t('tip.pleaseInput')"
                  name="linkNewName"
                  v-model="newName"
                />
              </div>
            </div>
            <div class="text-end">
              <button
                class="btn btn-primary btn-lg btn-radius mb-6 mt-10"
                @click="update"
                :disabled="isUpdating"
              >
                <span v-if="isUpdating"
                  ><MoonLoader class="mt-1" :color="'#ffffff'"
                /></span>
                <span v-else>{{ $t("action.update") }}</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { showModal, hideModal } from "@/core/helpers/dom";
import { Field } from "vee-validate";
import IbService from "../../services/IbService";
import MoonLoader from "vue-spinner/src/PulseLoader.vue";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import notification from "@/core/plugins/notification";
import { isMobile } from "@/core/config/WindowConfig";

const emits = defineEmits<{
  (e: "fetchData"): void;
}>();

const isUpdating = ref(false);
const item = ref({} as any);
const newName = ref("");
const LinkEditModalRef = ref<null | HTMLElement>(null);
const isDefault = ref(0);

const setDefaultLink = async () => {
  if (isDefault.value == 0) {
    isDefault.value = 1;
    return;
  }
  isUpdating.value = true;
  try {
    await IbService.setDefaultClient(item.value.code);
    notification.success();
    emits("fetchData");
    hide();
  } catch (error) {
    console.log(error);
    notification.danger();
  }
  isUpdating.value = false;
};

const update = async () => {
  isUpdating.value = true;
  if (!newName.value) {
    isUpdating.value = false;
    return;
  }
  try {
    await IbService.updateIbLink(item.value.code, {
      name: newName.value,
    });
    notification.success();
    emits("fetchData");
    hide();
  } catch (error) {
    console.log(error);
    notification.danger();
  }
  isUpdating.value = false;
};

const show = async (_item: any) => {
  isUpdating.value = false;
  showModal(LinkEditModalRef.value);
  item.value = _item;
  newName.value = "";
  isDefault.value = _item.isDefault;
};

const hide = () => {
  hideModal(LinkEditModalRef.value);
};

defineExpose({
  hide,
  show,
});
</script>

<style scoped>
.ibLinkDetailBackdrop {
  background-color: rgba(0, 0, 0, 0.5);
}
:deep .is-text {
  padding-left: 5px;
  padding-right: 5px;
}
</style>
