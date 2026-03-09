<template>
  <div
    class="modal fade"
    tabindex="-1"
    aria-hidden="true"
    aria-modal="true"
    ref="newTargetModalRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-850px">
      <div
        class="modal-content"
        v-if="details != null"
        style="min-height: 700px"
      >
        <div
          class="form fv-plugins-bootstrap5 fv-plugins-framework w-100"
          id="kt_modal_add_event_form"
          :rules="rules"
        >
          <!-- header section -->
          <div class="modal-header border-0 px-sm-10 px-4 pb-3">
            <h2 class="fs-2 event-title">
              {{ details.title }}
            </h2>
            <div id="kt_modal_add_event_close" data-bs-dismiss="modal">
              <span class="svg-icon svg-icon-1">
                <inline-svg src="/images/icons/arrows/arr061.svg" />
              </span>
            </div>
          </div>
          <!-- content section -->
          <div class="modal-body px-sm-10 px-4 w-100 overflow-auto">
            <div class="w-100 notice-content" v-html="details.content"></div>
          </div>
          <!-- checkbox section -->
          <div
            class="d-flex flex-row align-items-center gap-2 mx-10 mb-sm-10 mb-4"
          >
            <input
              class=""
              :checked="checkedRef"
              :disabled="isRegistering"
              type="checkbox"
              name="user_management_read"
              @change="(e) => handleChange(e)"
            />
            <span>同意活动规则</span>
          </div>
          <!-- agree button -->
          <div
            class="d-flex align-items-center justify-content-end m-sm-10 m-4"
          >
            <button
              v-if="!checkedRef"
              class="btn btn-sm btn-primary d-flex align-items-center gap-2"
              disabled
            >
              确认报名
            </button>
            <LoadingButton
              v-if="checkedRef"
              :is-loading="isRegistering"
              :save-title="'确认报名'"
              :saved-title="'正在报名...'"
              class="btn btn-sm btn-primary d-flex align-items-center gap-2"
              @click.prevent="handleRegister"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { hideModal, showModal } from "@/core/helpers/dom";
import LoadingButton from "@/components/buttons/LoadingButton.vue";

const newTargetModalRef = ref<null | HTMLElement>(null);
const rules = ref();
const details = ref<any>(null);
const checkedRef = ref<boolean>(false);
const handleChange = (e: Event) => {
  if (e instanceof Event) {
    const inputElement = e.target as HTMLInputElement;
    checkedRef.value = inputElement.checked;
  }
};
const show = async (_detail: any) => {
  checkedRef.value = false;
  isRegistering.value = false;
  details.value = _detail;
  showModal(newTargetModalRef.value);
};

const hide = () => {
  checkedRef.value = false;
  isRegistering.value = false;
  hideModal(newTargetModalRef.value);
};

defineExpose({
  show,
  hide,
});
const isRegistering = ref(false);
const handleRegister = async () => {
  isRegistering.value = true;
  try {
    console.log("register function here");
    isRegistering.value = false;
    // success then reload page.
    window.location.reload();
  } catch (error: any) {
    console.log(error);
  }
};
</script>

<style lang="scss">
.image-bg {
  aspect-ratio: 16/4;
  background-image: url("/images/bg/bcr-points-mall-banner.jpeg");
  background-size: cover;
  background-repeat: no-repeat;
}

.el-select {
  width: 100%;
}

.el-date-editor.el-input,
.el-date-editor.el-input__inner {
  width: 100%;
}

.notice-content table {
  border: 1px double #b3b3b3;
  border-collapse: collapse;
  border-spacing: 0;
  height: 100%;
  width: 100%;
}

.notice-content table td {
  border: 1px solid #bfbfbf;
  min-width: 2em;
  padding: 0.4em;
  overflow-wrap: break-word;
  position: relative;
}

.notice-content table td:first-child {
  padding: 0.4em;
}

.event-title {
  font-style: normal;
  font-weight: 600;
  font-size: 28px;
  line-height: 36px;
  color: #212121;
}

.event-detail {
  font-style: normal;
  font-weight: 600;
  font-size: 14px;
  line-height: 20px;
  color: #89939e;
}

.event-detail_value {
  font-style: normal;
  font-weight: 600;
  font-size: 14px;
  line-height: 20px;
  color: #212121;
}
</style>
