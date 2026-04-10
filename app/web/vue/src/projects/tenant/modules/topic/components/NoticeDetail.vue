<template>
  <div
    class="modal fade"
    tabindex="-1"
    data-bs-focus="false"
    aria-hidden="true"
    aria-modal="true"
    role="dialog"
    ref="noticeDetailShowRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-950px">
      <div class="modal-content px-10 py-10">
        <p class="fs-1">{{ detail["title"] }}</p>
        <div>
          <el-form
            :model="timeForm"
            :rules="timeRules"
            ref="timeFormRef"
            class="row w-100"
          >
            <el-form-item prop="effectiveFrom" class="col-sm-5">
              <div class="row w-100">
                <div class="col">
                  <div class="">
                    <label class="fs-6 fw-semibold mb-2 required">{{
                      $t("title.eventStartDate")
                    }}</label>
                    <el-form-item prop="startDate">
                      <el-date-picker v-model="startDate" type="date" />
                    </el-form-item>
                    <div
                      class="fv-plugins-message-container invalid-feedback"
                    ></div>
                  </div>
                </div>

                <div class="col">
                  <div class="fv-row mb-5">
                    <label class="fs-6 fw-semibold mb-2">{{
                      $t("title.eventStartTime")
                    }}</label>
                    <el-form-item prop="startTime">
                      <el-time-picker v-model="startTime" arrow-control />
                    </el-form-item>
                  </div>
                </div>
              </div>
            </el-form-item>
            <el-form-item prop="effectiveTo" class="col-sm-5">
              <div class="row w-100">
                <div class="col">
                  <div
                    class="fv-row mb-5 fv-plugins-icon-container fv-plugins-bootstrap5-row-valid"
                  >
                    <label class="fs-6 fw-semibold mb-2 required">{{
                      $t("title.eventEndDate")
                    }}</label>
                    <el-form-item prop="endDate">
                      <el-date-picker
                        v-model="endDate"
                        type="date"
                        :teleported="false"
                        name="eventName"
                      />
                    </el-form-item>
                    <div
                      class="fv-plugins-message-container invalid-feedback"
                    ></div>
                  </div>
                </div>

                <div class="col">
                  <div class="fv-row mb-5">
                    <label class="fs-6 fw-semibold mb-2">{{
                      $t("title.eventEndTime")
                    }}</label>
                    <el-form-item prop="endTime">
                      <el-time-picker
                        v-model="endTime"
                        :teleported="false"
                        arrow-control
                        prop="endTime"
                      />
                    </el-form-item>
                  </div>
                </div>
              </div>
            </el-form-item>
            <div class="col-sm-2">
              <div class="fv-row mb-5">
                <label v-if="!isMobile" class="fs-6 fw-semibold mb-5">
                  &nbsp;</label
                >
                <div
                  class="btn btn-primary d-block btn-sm"
                  @click.prevent="submitTime(timeFormRef)"
                >
                  {{ $t("title.editTime") }}
                </div>
              </div>
            </div>
          </el-form>
        </div>
        <ul
          v-if="!submited"
          class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
        >
          <li
            class="nav-item"
            v-for="(item, key, index) in detail['contents']"
            :key="index"
          >
            <button
              class="nav-link text-active-primary pb-4"
              :class="{ active: tab === key.toString() }"
              data-bs-toggle="tab"
              @click="changeTab(key, item)"
            >
              {{ key }}
            </button>
          </li>
          <li class="nav-item" v-if="!isAllLanguage">
            <button
              class="nav-link text-active-primary pb-4"
              :class="{ active: tab === 'new' }"
              data-bs-toggle="tab"
              @click="changeTab('new', null)"
            >
              {{ $t("action.new") }}
            </button>
          </li>
        </ul>

        <el-form
          class="demo-ruleForm"
          id="kt_modal_add_event_form"
          @submit.prevent="submit"
          :model="formData"
          :rules="formRules"
          ref="ruleFormRef"
        >
          <div class="fv-row mb-5">
            <label class="fs-6 fw-semibold mb-2 required">{{
              $t("fields.language")
            }}</label>
            <el-form-item prop="language" name="language">
              <el-select v-model="formData.language">
                <el-option
                  v-for="item in languageOptions"
                  :key="item['code']"
                  :label="item['name']"
                  :value="item['code']"
                />
              </el-select>
            </el-form-item>
          </div>

          <div class="fv-row mb-5 fv-plugins-icon-container">
            <label class="fs-6 fw-semibold required mb-2">{{
              $t("title.title")
            }}</label>
            <el-form-item prop="title">
              <el-input v-model="formData.title" type="text" name="title" />
            </el-form-item>
          </div>

          <div class="fv-row mb-5">
            <label class="fs-6 fw-semibold mb-2 required">{{
              $t("title.content")
            }}</label>
            <el-form-item name="content" prop="content">
              <!-- <ckeditor :editor="editor" v-model="formData.content"></ckeditor> -->
              <div class="w-100">
                <jodit-editor v-model="formData.content" />
              </div>
            </el-form-item>
          </div>
          <div class="text-end">
            <button :class="`btn btn-secondary me-3`" @click.prevent="close">
              {{ $t("action.close") }}
            </button>
            <button
              :class="`btn btn-primary me-3`"
              type="submit"
              :disabled="submited || isLoading"
              @click.prevent="submit(ruleFormRef)"
            >
              <span v-if="submited">
                {{ $t("action.waiting") }}
                <span
                  class="spinner-border h-15px w-15px align-middle text-gray-400"
                ></span>
              </span>
              <span v-else> {{ $t("action.submit") }} </span>
            </button>
          </div>
        </el-form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from "vue";
import { hideModal, showModal } from "@/core/helpers/dom";
import { LanguageTypes } from "@/core/types/LanguageTypes";
import TopicService from "../services/TopicService";
import { type FormRules, FormInstance } from "element-plus";
import { isMobile } from "@/core/config/WindowConfig";
import { ElNotification } from "element-plus";
import "jodit/build/jodit.min.css";
import { JoditEditor } from "jodit-vue";

const noticeDetailShowRef = ref<null | HTMLElement>(null);
const languageOptions = ref([]);
const isAllLanguage = ref(false);
const ruleFormRef = ref<FormInstance>();
const timeFormRef = ref<FormInstance>();
const detail = ref(Array<any>());
const isLoading = ref(true);
const submited = ref(false);

const startDate = ref<any>(new Date());
const startTime = ref<any>(new Date());
const endDate = ref<any>(new Date());
const endTime = ref<any>(new Date());

const formData = ref<any>({
  id: 0,
  title: "",
  language: "",
  content: "",
  author: "",
});

const timeForm = ref<any>({
  id: detail.value["id"],
  startDate: startDate,
  startTime: startTime,
  endDate: endDate,
  endTime: endTime,
});

const timeRules = reactive<FormRules>({
  startDate: [
    {
      required: true,
      message: "Please input effective start date",
      trigger: "blur",
    },
  ],
  startTime: [
    {
      required: true,
      message: "Please input effective start time",
      trigger: "blur",
    },
  ],
  endDate: [
    {
      required: true,
      message: "Please input effective end date",
      trigger: "blur",
    },
  ],
  endTime: [
    {
      required: true,
      message: "Please input effective end time",
      trigger: "blur",
    },
  ],
});

const formRules = reactive<FormRules>({
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
  content: [
    {
      required: true,
      message: "Please input content",
      trigger: "blur",
    },
  ],
});

const show = (data) => {
  startDate.value = data.effectiveFrom;
  startTime.value = data.effectiveFrom;
  endDate.value = data.effectiveTo;
  endTime.value = data.effectiveTo;
  detail.value = data;

  let keys = Object.keys(detail.value["contents"]);
  if (keys.length > 0) {
    changeTab(keys[0], detail.value["contents"][keys[0]]);
  } else {
    changeTab("new", null);
  }
  isAllLanguage.value = keys.length >= LanguageTypes.all.length;
  showModal(noticeDetailShowRef.value);
  isLoading.value = false;
};

const tab = ref("new");

const changeTab = (_tab, item) => {
  tab.value = _tab;

  if (item) {
    formData.value = {
      title: item.title,
      content: item.content,
      language: item.language,
      id: item.id,
    };
    languageOptions.value = LanguageTypes.all.filter(
      (langItem) => langItem.code == item.language
    );
  } else {
    formData.value = {
      title: "",
      subtitle: "",
      content: "",
      language: "",
      effectiveTo: "",
      id: 0,
    };
    languageOptions.value = LanguageTypes.all.filter(
      (langItem) => detail.value.contents[langItem.code] == undefined
    );
  }
};
const emits = defineEmits<{
  (e: "eventSubmit"): void;
}>();

const updateNoticeLanguage = async () => {
  try {
    const res = await TopicService.updateNoticeLanguage(
      detail.value["id"],
      formData.value.id,
      formData.value
    );
    notification(1);
    return res;
  } catch (e) {
    notification(0);
  }
};

const createNoticeLanguage = async () => {
  try {
    const res = await TopicService.createNoticeLanguage(
      detail.value["id"],
      formData.value
    );
    notification(1);
    return res;
  } catch (e) {
    notification(0);
  }
  // const res = await TopicService.createNoticeLanguage(
  //   detail.value["id"],
  //   formData.value
  // );
};
const close = () => {
  hideModal(noticeDetailShowRef.value);
};

const submit = async (formEl: FormInstance | undefined) => {
  submited.value = true;

  try {
    await formEl?.validate(async (valid, fields) => {
      if (valid) {
        if (formData.value.id == 0) {
          const res = await createNoticeLanguage();
          detail.value.contents[formData.value.language] = res;
          isAllLanguage.value =
            Object.keys(detail.value.contents).length >=
            LanguageTypes.all.length;

          changeTab(formData.value.language, res);
        } else {
          const res = await updateNoticeLanguage();
          detail.value.contents[formData.value.language] = res;
        }
        emits("eventSubmit");
      } else {
        console.log("error submit!", fields);
      }
    });
  } catch (e) {
    notification(0);
  }

  submited.value = false;
};
const concatDates = (d1: Date, d2: Date) => {
  if (!d1 || !d2) return "";
  const newDate = new Date(d1);
  const d2_new = new Date(d2);
  newDate.setHours(d2_new.getHours());
  newDate.setMinutes(d2_new.getMinutes());
  newDate.setSeconds(d2_new.getSeconds());

  return newDate;
};

const submitTime = async (formEl: FormInstance | undefined) => {
  try {
    await formEl?.validate(async (valid, fields) => {
      if (valid) {
        const detailForm = {
          type: 2,
          effectiveFrom: concatDates(startDate.value, startTime.value),
          effectiveTo: concatDates(endDate.value, endTime.value),
        };
        await TopicService.updateNoticeTime(detail.value.id, detailForm);
        notification(1);
        emits("eventSubmit");
      } else {
        console.log("error submit!", fields);
      }
    });
  } catch (e) {
    notification(0);
  }
};
const notification = (type: number) => {
  if (type == 1) {
    ElNotification({
      title: "Success",
      message: "Update success!",
      type: "success",
      offset: 100,
    });
  } else {
    ElNotification({
      title: "Error",
      message: "Update failed!",
      type: "error",
      offset: 100,
    });
  }
};
defineExpose({
  show,
});
</script>
<style scoped lang="scss">
:deep(.ck-reset) {
  width: 100% !important;
}
</style>
