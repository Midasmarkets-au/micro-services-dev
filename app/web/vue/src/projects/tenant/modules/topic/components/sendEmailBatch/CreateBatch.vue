<template>
  <el-drawer v-model="drawerRef" direction="rtl" size="90%">
    <template #header>
      <h4>Create Batch</h4>
    </template>
    <template #default>
      <el-form label-position="top" :model="formData">
        <div class="d-flex gap-3">
          <el-form-item label="Topic ID">
            <el-input
              v-model="formData.topicId"
              class="w-125px"
              :disabled="isLoading"
            />
          </el-form-item>
          <el-form-item label="Topic Key">
            <el-input
              v-model="formData.topicKey"
              class="w-250px"
              placeholder="If send same email, use same key"
              :disabled="isLoading"
            />
          </el-form-item>
          <el-form-item label="Email Receivers">
            <el-input
              :disabled="isLoading"
              class="w-500px"
              v-model="receiverEmails"
              :autosize="{ minRows: 2, maxRows: 4 }"
              type="textarea"
              placeholder="Enter email addresses separated by comma"
            />
          </el-form-item>

          <el-form-item label="Select Site">
            <el-select
              v-model="formData.siteId"
              :disabled="isLoading"
              class="w-250px"
            >
              <el-option
                v-for="(item, index) in ConfigSiteTypesSelections"
                :key="index"
                :label="item.label"
                :value="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
        </div>
        <hr />
        <el-form-item label="Select Languages">
          <el-checkbox-group v-model="languageList">
            <el-checkbox
              v-for="(content, index) in LanguageTypes.all"
              :key="index"
              :label="content.code"
              :value="content.code"
              :disabled="isLoading"
              @change="onLanguageListChange(content.code)"
            >
              {{ content.code }}
            </el-checkbox>
          </el-checkbox-group>
        </el-form-item>

        <el-tabs type="card">
          <el-tab-pane
            :label="index"
            v-for="(item, index) in formData.contents"
            :key="index"
          >
            <div class="row">
              <div class="col-6">
                <el-form-item label="Title">
                  <el-input v-model="item.title" :disabled="isLoading" />
                </el-form-item>
                <el-form-item label="Subtitle">
                  <el-input v-model="item.subTitle" :disabled="isLoading" />
                </el-form-item>
                <el-form-item label="Content">
                  <div class="w-100">
                    <jodit-editor
                      v-model="item.content"
                      :disabled="isLoading"
                    />
                  </div>
                </el-form-item>
              </div>
              <div class="col-6">
                <iframe
                  :srcdoc="refreshHtml(item)"
                  width="100%"
                  height="100%"
                ></iframe>
              </div>
            </div>
          </el-tab-pane>
        </el-tabs>
      </el-form>
    </template>

    <template #footer>
      <div style="flex: auto">
        <el-button
          @click="drawerRef = false"
          type="warning"
          :disabled="isLoading"
          >Cancel</el-button
        >
        <el-button type="success" @click="submit" :loading="isLoading"
          >Submit</el-button
        >
      </div>
    </template>
  </el-drawer>
</template>
<script setup lang="ts">
import { ref } from "vue";
import { LanguageTypes } from "@/core/types/LanguageTypes";
import "jodit/build/jodit.min.css";
import { JoditEditor } from "jodit-vue";
import TopicService from "../../services/TopicService";
import notification from "@/core/plugins/notification";
import { ConfigSiteTypesSelections } from "@/core/types/SiteTypes";

const isLoading = ref(false);
const drawerRef = ref(false);
const languageList = ref<any>(["en-us", "zh-cn"]);
const receiverEmails = ref<any>("");
const formData = ref({
  topicId: 10003,
  topicKey: null,
  overwrite: true,
  siteId: null,
  receiverEmails: {},
  contents: {
    "en-us": {
      title: "",
      subTitle: "",
      content: "",
      language: "en-us",
    },
    "zh-cn": {
      title: "",
      subTitle: "",
      content: "",
      language: "zh-cn",
    },
  },
});
const emits = defineEmits<{
  (e: "eventSubmit"): void;
}>();

const onLanguageListChange = (langCode) => {
  if (languageList.value.includes(langCode)) {
    formData.value.contents[langCode] = {
      title: "",
      subTitle: "",
      content: "",
      language: langCode,
    };
  } else {
    delete formData.value.contents[langCode];
  }
};

const refreshHtml = (item) => {
  return item.content;
};

const submit = async () => {
  isLoading.value = true;
  try {
    var unTrimmedEmailList = receiverEmails.value.split(",");
    var trimmedEmailList = unTrimmedEmailList
      .map((email) => email.trim())
      .filter((email) => email !== "");
    formData.value.receiverEmails = trimmedEmailList;
    console.log(formData.value);
    await TopicService.createEmailBatch(formData.value);
    drawerRef.value = false;
    emits("eventSubmit");
    reset();
    notification.success();
  } catch (error) {
    console.error(error);
    notification.danger();
  } finally {
    isLoading.value = false;
  }
};

const reset = () => {
  receiverEmails.value = "";
  languageList.value = ["en-us", "zh-cn"];
  formData.value = {
    topicId: 10003,
    overwrite: true,
    receiverEmails: {},
    contents: {
      "en-us": {
        title: "",
        subTitle: "",
        content: "",
      },
      "zh-cn": {
        title: "",
        subTitle: "",
        content: "",
      },
    },
  };
};

const show = (_data: any) => {
  drawerRef.value = true;
  if (_data) {
    formData.value = _data;
    receiverEmails.value = _data.receiverEmails.join(",");
    languageList.value = Object.keys(_data.contents);
  }
};

defineExpose({
  show,
});
</script>
