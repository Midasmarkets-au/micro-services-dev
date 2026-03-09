<template>
  <div
    class="modal fade ibLinkDetailBackdrop"
    id="kt_modal_iblibk_detail"
    tabindex="-1"
    aria-hidden="true"
    ref="LinkEditModalRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-750px">
      <div class="modal-content ps-5 pe-5">
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
                @change="setDefaultLink()"
              />
            </div>
          </div>

          <div data-bs-dismiss="modal">
            <span class="svg-icon svg-icon-1">
              <inline-svg src="/images/icons/arrows/arr061.svg" />
            </span>
          </div>
        </div>
        <!-- ------------------------------------------------------------------ -->

        <div class="form-outer">
          <div class="card-body py-0 mt-13">
            <div class="row">
              <div class="col-6">
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
              <div class="col-6">
                <div class="d-flex align-items-center">
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

            <div class="row d-flex flex-center">
              <button
                class="btn btn-primary btn-lg btn-radius mb-10 mt-10"
                :class="isMobile ? 'col-6' : 'col-3'"
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
import SalesService from "@/projects/client/modules/sales/services/SalesService";
import MoonLoader from "vue-spinner/src/PulseLoader.vue";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import notification from "@/core/plugins/notification";
import { isMobile } from "@/core/config/WindowConfig";
const emits = defineEmits<{
  (e: "fetchData", page: number): void;
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
    await SalesService.updateIbDefaultCode(item.value.code);
    notification.success();
    emits("fetchData", 1);
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
    await SalesService.updateIbLink(item.value.id, {
      status: item.value.status,
      name: newName.value,
    });
    notification.success();
    emits("fetchData", 1);
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
</style>
