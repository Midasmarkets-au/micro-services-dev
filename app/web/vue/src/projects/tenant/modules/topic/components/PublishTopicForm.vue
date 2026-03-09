<template>
  <div
    class="modal fade"
    tabindex="-1"
    aria-hidden="true"
    aria-modal="true"
    role="dialog"
    ref="newTargetModalRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-650px">
      <div class="modal-content">
        <el-form
          class="form fv-plugins-bootstrap5 fv-plugins-framework"
          id="kt_modal_add_event_form"
          @submit.prevent="submit"
          :model="formData"
          :rules="rules"
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

          <div class="modal-body py-10 px-lg-17">
            <div class="fv-row mb-5 fv-plugins-icon-container">
              <label class="fs-6 fw-semibold required mb-2">{{
                $t("title.title")
              }}</label>
              <el-form-item prop="title">
                <el-input v-model="formData.title" type="text" name="title" />
              </el-form-item>
              <div class="fv-plugins-message-container invalid-feedback"></div>
            </div>

            <!-- <div class="fv-row mb-5">
              <label class="fs-6 fw-semibold mb-2">{{
                $t("title.TopicType")
              }}</label>

              <el-select v-model="formData.type" placeholder="Select">
                <el-option
                  v-for="({ label, value }, index) in topicSelections"
                  :key="index"
                  :label="label"
                  :value="value"
                />
              </el-select>
            </div> -->

            <div class="fv-row mb-5">
              <label class="fs-6 fw-semibold mb-2">{{
                $t("fields.language")
              }}</label>
              <el-form-item prop="language">
                <el-select
                  v-model="formData.language"
                  :placeholder="$t('action.select')"
                >
                  <el-option
                    v-for="({ label, value }, index) in languageSelections"
                    :key="index"
                    :label="label"
                    :value="value"
                  /> </el-select
              ></el-form-item>
            </div>

            <div class="fv-row mb-5">
              <label class="fs-6 fw-semibold mb-2">{{
                $t("fields.author")
              }}</label>
              <el-form-item prop="author">
                <el-input
                  v-model="formData.author"
                  type="text"
                  placeholder=""
                  name="author"
              /></el-form-item>
            </div>

            <div class="fv-row mb-5">
              <label class="fs-6 fw-semibold mb-2">{{
                $t("title.content")
              }}</label>

              <el-form-item prop="content">
                <!-- <el-input
                  v-model="formData.content"
                  type="textarea"
                  placeholder=""
                  name="content"
                >
                </el-input> -->

                <div class="w-100">
                  <Editor
                    v-model="formData.content"
                    api-key="dgpx6ul0v883nikc809en1axx8z8rsjkha2fsobcyki3ad3n"
                    :init="editorInitOptions"
                  />
                </div>
              </el-form-item>
            </div>

            <div class="fv-row mb-5">
              <label class="form-check form-check-custom form-check-solid">
                <el-checkbox v-model="allDay" type="checkbox" />
                <span class="form-check-label fw-semobold">{{
                  $t("title.allDay")
                }}</span>
              </label>
            </div>

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
import { reactive, ref, watch } from "vue";
import { hideModal, showModal } from "@/core/helpers/dom";
import { LanguageCodes } from "@/core/types/LanguageTypes";
import i18n from "@/core/plugins/i18n";
import { TopicTypes } from "@/core/types/TopicTypes";
import UserService from "../../users/services/UserService";
import Editor from "@tinymce/tinymce-vue";
import { FormInstance, FormRules } from "element-plus";

const { t } = i18n.global;
const ruleFormRef = ref<FormInstance>();
const newTargetModalRef = ref<null | HTMLElement>(null);
const isLoading = ref<boolean>(false);
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

const formData = ref<any>({
  title: "",
  type: TopicTypes.Notice,
  language: "",
  content: "",
  author: "",
  effectiveFrom: concatDates(startDate.value, startTime.value),
  effectiveTo: concatDates(endDate.value, endTime.value),
});

const languageSelections = [
  {
    label: t(`type.language.${LanguageCodes.enUS}`),
    value: LanguageCodes.enUS,
  },
  {
    label: t(`type.language.${LanguageCodes.zhCN}`),
    value: LanguageCodes.zhCN,
  },
  {
    label: t(`type.language.${LanguageCodes.zhHK}`),
    value: LanguageCodes.zhHK,
  },
];

const show = () => {
  // initialize topic selectins list by TopicType enum
  ruleFormRef.value?.resetFields();
  formData.value = {
    title: "",
    type: TopicTypes.Notice,
    language: "",
    content: "",
    author: "",
    effectiveFrom: concatDates(startDate.value, startTime.value),
    effectiveTo: concatDates(endDate.value, endTime.value),
  };

  showModal(newTargetModalRef.value);
};

const hide = () => {
  hideModal(newTargetModalRef.value);
};

watch(
  [startDate, startTime],
  ([startDate, startTime]) => {
    if (!allDay.value && (!startDate || !startTime)) {
      formData.value.effectiveFrom = null;
      return;
    }
    formData.value.effectiveFrom = concatDates(startDate, startTime);
  },
  { immediate: true }
);

watch(
  [endDate, endTime],
  ([endDate, endTime]) => {
    if (!allDay.value && (!endDate || !endTime)) {
      formData.value.effectiveTo = null;
      return;
    }
    formData.value.effectiveTo = concatDates(endDate, endTime);
  },
  { immediate: true }
);

const emits = defineEmits<{
  (e: "eventSubmit", type: TopicTypes): void;
}>();

const submit = async () => {
  if (!ruleFormRef.value) return;
  await ruleFormRef.value.validate(async (valid, fields) => {
    if (valid) {
      isLoading.value = true;
      try {
        await UserService.postEventTopic(formData.value);
        emits("eventSubmit", formData.value.type as unknown as TopicTypes);
        isLoading.value = false;
        hide();
      } catch (error: any) {
        console.log(error.message);
      }
    } else {
      console.log("error submit!!", fields);
    }
  });
};

const rules = ref<FormRules>({
  title: [
    {
      required: true,
      message: "Please input title",
      trigger: "blur",
    },
  ],
  language: [
    {
      required: true,
      message: "Please select language",
      trigger: "blur",
    },
  ],
  type: [
    {
      required: true,
      message: "Please select type",
      trigger: "blur",
    },
  ],
  content: [
    {
      required: true,
      message: "Please input content",
      trigger: "blur",
    },
  ],
  author: [
    {
      required: true,
      message: "Please input author",
      trigger: "blur",
    },
  ],
  effectiveFrom: [
    {
      required: true,
      message: "Please input effective from",
      trigger: "blur",
    },
  ],
  effectiveTo: [
    {
      required: true,
      message: "Please input effective to",
      trigger: "blur",
    },
  ],
});

const editorInitOptions = ref({
  menubar: false,
  plugins: "lists link image emoticons table",
  toolbar:
    "styleselect | bold italic | alignleft aligncenter alignright alignjustify | table tabledelete | bullist numlist | link image emoticons",
});

defineExpose({
  show,
});
</script>

<style lang="scss">
.el-select {
  width: 100%;
}

.el-date-editor.el-input,
.el-date-editor.el-input__inner {
  width: 100%;
}
</style>
