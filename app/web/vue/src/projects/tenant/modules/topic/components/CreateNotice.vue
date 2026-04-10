<template>
  <div
    class="modal fade"
    tabindex="-1"
    aria-hidden="true"
    aria-modal="true"
    role="dialog"
    ref="createNoticeModalRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-650px">
      <div class="modal-content">
        <el-form
          class="form fv-plugins-bootstrap5 fv-plugins-framework"
          id="kt_modal_add_event_form"
          @submit.prevent="createNotice"
          :model="detailForm"
          :rules="noticeFormRules"
          ref="ruleFormRef"
        >
          <div class="modal-header">
            <h2 class="fw-bold">{{ $t("title.addNewTopic") }}</h2>
            <div
              class="btn btn-icon btn-sm btn-active-icon-primary"
              id="kt_modal_add_event_close"
              data-bs-dismiss="modal"
            >
              <span class="svg-icon svg-icon-1">
                <inline-svg src="/images/icons/arrows/arr061.svg" />
              </span>
            </div>
          </div>
          <div class="modal-body flex-center">
            <el-form-item :label="$t('fields.title')" prop="title">
              <el-input v-model="detailForm.title" />
            </el-form-item>
            <el-form-item prop="effectiveFrom">
              <div class="row w-100">
                <div class="col">
                  <div
                    class="fv-row mb-5 fv-plugins-icon-container fv-plugins-bootstrap5-row-valid"
                  >
                    <label class="fs-6 fw-semibold mb-2 required">{{
                      $t("title.eventStartDate")
                    }}</label>
                    <el-date-picker
                      v-model="startDate"
                      type="date"
                      :teleported="false"
                      name="eventStartDate"
                    />
                    <div
                      class="fv-plugins-message-container invalid-feedback"
                    ></div>
                  </div>
                </div>

                <div v-if="!allDay" class="col">
                  <div class="fv-row mb-5">
                    <label class="fs-6 fw-semibold mb-2">{{
                      $t("title.eventStartTime")
                    }}</label>

                    <el-time-picker
                      v-model="startTime"
                      arrow-control
                      :teleported="false"
                      placeholder=""
                    />
                  </div>
                </div>
              </div>
            </el-form-item>

            <el-form-item prop="effectiveTo">
              <div class="row w-100">
                <div class="col">
                  <div
                    class="fv-row mb-5 fv-plugins-icon-container fv-plugins-bootstrap5-row-valid"
                  >
                    <label class="fs-6 fw-semibold mb-2 required">{{
                      $t("title.eventEndDate")
                    }}</label>
                    <el-date-picker
                      v-model="endDate"
                      type="date"
                      :teleported="false"
                      name="eventName"
                    />
                    <div
                      class="fv-plugins-message-container invalid-feedback"
                    ></div>
                  </div>
                </div>

                <div v-if="!allDay" class="col">
                  <div class="fv-row mb-5">
                    <label class="fs-6 fw-semibold mb-2">{{
                      $t("title.eventEndTime")
                    }}</label>
                    <el-time-picker
                      v-model="endTime"
                      :teleported="false"
                      arrow-control
                      placeholder=""
                    />
                  </div>
                </div>
              </div>
            </el-form-item>
            <el-form-item :label="$t('fields.category')" prop="category">
              <el-select
                v-model="detailForm.category"
                style="width: 100%"
                :teleported="false"
              >
                <el-option
                  v-for="item in categorySelections"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
                />
              </el-select>
            </el-form-item>
          </div>
          <div class="modal-footer flex-center">
            <button
              data-bs-dismiss="modal"
              type="reset"
              id="kt_modal_add_event_cancel"
              class="btn btn-light me-3"
            >
              {{ $t("status.cancel") }}
            </button>
            <button
              :data-kt-indicator="isLoading ? 'on' : null"
              class="btn btn-lg btn-primary"
              type="submit"
            >
              <span v-if="!isLoading" class="indicator-label">
                {{ $t("action.submit") }}
                <span class="svg-icon svg-icon-3 ms-2 me-0">
                  <inline-svg src="/images/icons/arrows/arr064.svg" />
                </span>
              </span>
              <span v-if="isLoading" class="indicator-progress">
                {{ $t("title.inProgress") }}
                <span
                  class="spinner-border spinner-border-sm align-middle ms-2"
                ></span>
              </span>
            </button>
          </div>
        </el-form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from "vue";
import { TopicTypes } from "@/core/types/TopicTypes";
import { TopicCategoryTypes } from "@/core/types/TopicCategoryTypes";
import GlobalService from "@/projects/tenant/modules/topic/services/TopicService";
import type { FormRules } from "element-plus";
import { hideModal, showModal } from "@/core/helpers/dom";

const isLoading = ref(true);
const submited = ref(false);
const createNoticeModalRef = ref<null | HTMLElement>(null);
const emits = defineEmits<{
  (e: "eventSubmit", type: TopicTypes): void;
}>();

const allDay = ref<boolean>(false);
const startDate = ref<any>(new Date());
const startTime = ref<any>(new Date());
const endDate = ref<any>(new Date());
const endTime = ref<any>(new Date());

const concatDates = (d1: Date, d2: Date) => {
  if (!d1 || !d2) return "";
  return allDay.value
    ? new Date(d1.getFullYear(), d1.getMonth(), d1.getDate(), 0, 0, 0)
    : new Date(
        d1?.getFullYear(),
        d1?.getMonth(),
        d1?.getDate(),
        d2?.getHours(),
        d2?.getMinutes(),
        d2?.getSeconds()
      );
};

const detailForm = ref<any>({
  title: "",
  type: TopicTypes.Notice,
  category: TopicCategoryTypes.Information,
  language: "en-us",
  content: "content",
  author: "system",
  effectiveFrom: concatDates(startDate.value, startTime.value),
  effectiveTo: concatDates(endDate.value, endTime.value),
});

const categorySelections = [
  { label: "Activity", value: TopicCategoryTypes.Activity },
  { label: "Information", value: TopicCategoryTypes.Information },
];

const noticeFormRules = reactive<FormRules>({
  title: [{ required: true, message: "Please input the Notice template key" }],
  category: [{ required: true, message: "Please select category" }],
});

const createNotice = async () => {
  submited.value = true;
  await GlobalService.createNotice(detailForm.value);
  emits("eventSubmit", TopicTypes.Notice);
  submited.value = false;
  hide();
};

watch(
  [startDate, startTime],
  ([startDate, startTime]) => {
    if (!allDay.value && (!startDate || !startTime)) {
      detailForm.value.effectiveFrom = null;
      return;
    }
    detailForm.value.effectiveFrom = concatDates(startDate, startTime);
  },
  { immediate: true }
);

watch(
  [endDate, endTime],
  ([endDate, endTime]) => {
    if (!allDay.value && (!endDate || !endTime)) {
      detailForm.value.effectiveTo = null;
      return;
    }
    detailForm.value.effectiveTo = concatDates(endDate, endTime);
  },
  { immediate: true }
);

const show = () => {
  showModal(createNoticeModalRef.value);
  isLoading.value = false;
};

const hide = () => {
  hideModal(createNoticeModalRef.value);
};

defineExpose({
  show,
});
</script>
