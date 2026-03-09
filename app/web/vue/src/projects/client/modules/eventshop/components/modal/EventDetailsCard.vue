<template>
  <div
    class="modal fade"
    tabindex="-1"
    aria-hidden="true"
    aria-modal="true"
    role="dialog"
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
          style="max-height: 80vh; overflow: auto"
        >
          <!-- header section -->
          <div class="modal-header border-0 px-sm-10 px-5 align-items-start">
            <h2 class="event-title fs-2">
              {{ details.title }}
              <div class="d-flex fw-normal fs-7 pt-3" style="color: #b1b1b1">
                <span>{{ $t("fields.postedOn") }}:&nbsp;</span>
                <TimeShow
                  :date-iso-string="details.updatedOn"
                  type="customCSS"
                />
              </div>
            </h2>
            <div id="kt_modal_add_event_close" data-bs-dismiss="modal">
              <span class="svg-icon svg-icon-1">
                <inline-svg src="/images/icons/arrows/arr061.svg" />
              </span>
            </div>
          </div>
          <!-- <img :src="details.image" class="image-bg" /> -->
          <el-image :src="imgUrl" class="mx-sm-10 mx-3 image-bg" />
          <!-- date section -->
          <div class="d-flex flex-row mx-sm-10 mx-5 gap-5 mt-5">
            <div class="flex-column d-flex gap-2">
              <span class="event-detail">{{ $t("fields.startDate") }}</span>
              <span class="event-detail_value"
                ><TimeShow :date-iso-string="details.startOn" type="customCSS"
              /></span>
            </div>
            <div class="flex-column d-flex gap-2">
              <span class="event-detail">{{ $t("fields.activityCycle") }}</span>
              <span class="event-detail_value">{{
                $t("fields.longTerm")
              }}</span>
            </div>
          </div>
          <!-- content section -->
          <div class="modal-body px-sm-10 px-5 w-100 overflow-auto">
            <div
              class="w-100 notice-content"
              v-html="details.description"
            ></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { hideModal, showModal } from "@/core/helpers/dom";

const newTargetModalRef = ref<null | HTMLElement>(null);
const rules = ref();
const details = ref<any>(null);
var imgUrl = "/images/bg/bcr-points-mall-banner_new_theme.png";
const show = async (_detail: any, _imgUrl?: any) => {
  details.value = _detail;
  if (_imgUrl) {
    imgUrl = _imgUrl;
  }

  showModal(newTargetModalRef.value);
};

const hide = () => {
  hideModal(newTargetModalRef.value);
};

defineExpose({
  show,
  hide,
});
</script>

<style lang="scss">
.image-bg {
  aspect-ratio: 16/6;
  // background-image: var(--banner-bg-image);
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
