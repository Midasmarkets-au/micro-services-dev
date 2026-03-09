<template>
  <el-drawer
    v-model="drawer"
    :title="title"
    direction="ltr"
    size="90%"
    :before-close="close"
  >
    <div class="d-flex gap-4 mb-6">
      <div>
        <el-select v-model="selectedLanguage" :disabled="!canAddLang">
          <el-option
            v-for="item in languageMenu"
            :key="item.code"
            :label="item.code + '   (' + item.name + ')'"
            :value="item.code"
          />
        </el-select>
      </div>
      <el-button type="warning" @click="addLang" :disabled="!canAddLang">
        {{ $t("action.addALanguage") }}</el-button
      >
    </div>
    <div>
      <el-tabs type="card">
        <el-tab-pane
          :label="index"
          v-for="(item, index) in data.contents"
          :key="index"
        >
          <div class="row">
            <div class="col-6">
              <div class="d-flex align-items-center gap-7 mb-4">
                <label>{{ $t("fields.title") }}</label>
                <el-input v-model="item.title" class="w-500px"></el-input>
                <div></div>
              </div>
              <div class="d-flex align-items-center gap-4 mb-4">
                <label>{{ $t("fields.subtitle") }}</label>
                <el-input
                  :label="$t('fields.subtitle')"
                  v-model="item.subtitle"
                  class="w-500px"
                ></el-input>
              </div>
              <div class="overflow-auto h-600px">
                <el-input
                  v-model="item.content"
                  type="textarea"
                  :autosize="{ minRows: 20 }"
                />
              </div>
            </div>
            <div class="col-6">
              <iframe
                :srcdoc="refreshHtml(item)"
                width="100%"
                height="100%"
              ></iframe>
            </div>
          </div>
          <div class="row">
            <div class="col-6 mt-4 text-end">
              <el-button
                type="primary"
                plain
                :icon="Message"
                @click.prevent="sendTest(data.title, item.language)"
                :disabled="submitted || isLoading"
                >Test</el-button
              >
              <el-button
                type="danger"
                @click="close"
                :disabled="submitted || isLoading"
                >{{ $t("action.cancel") }}</el-button
              >
              <el-button
                type="success"
                @click="submit(item)"
                :loading="submitted || isLoading"
                :disabled="submitted || isLoading"
                >{{ $t("action.submit") }}</el-button
              >
            </div>
          </div>
        </el-tab-pane>
      </el-tabs>
    </div>
  </el-drawer>
  <SendTestEmail ref="sendTestEmailRef" />
</template>

<script setup lang="ts">
import { ref, computed } from "vue";
import { LanguageTypes } from "@/core/types/LanguageTypes";
import GlobalService from "../../../services/TenantGlobalService";
import { Message } from "@element-plus/icons-vue";
import SendTestEmail from "./SendTestEmail.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const isLoading = ref(false);
const submitted = ref(false);
const drawer = ref(false);

const data = ref(Array<any>());
const title = ref("");
const languageMenu = ref([]);
const selectedLanguage = ref("");

const layoutTemplate = ref({});
const sendTestEmailRef = ref<InstanceType<typeof SendTestEmail>>();

const close = () => {
  languageMenu.value = [];
  selectedLanguage.value = "";
  layoutTemplate.value = {};
  drawer.value = false;
};
const show = (detail, defaultLayout) => {
  data.value = detail;
  layoutTemplate.value = defaultLayout;
  title.value = detail.title;
  getLanguageMenu(detail.contents);
  drawer.value = true;
};
const refreshHtml = (item) => {
  if (item.language in layoutTemplate.value.contents) {
    var t = layoutTemplate.value.contents[item.language].content;
    t = t.replace("{{Title}}", item.title);
    t = t.replace("{{Subtitle}}", item.subtitle);
    t = t.replace("{{Content}}", item.content);
    return t;
  } else {
    return item.content;
  }
};

const sendTest = (title, lang) => {
  sendTestEmailRef.value.show(title, lang);
};

const getLanguageMenu = (detailContents) => {
  const langKeys = Object.keys(detailContents);
  languageMenu.value = LanguageTypes.all.filter(
    (item) => !langKeys.includes(item.code)
  );
  if (languageMenu.value.length > 0) {
    selectedLanguage.value = languageMenu.value[0].code;
  }
};

const addLang = () => {
  if (Object.keys(data.value.contents).length < LanguageTypes.all.length) {
    data.value.contents[selectedLanguage.value] = {
      id: 0,
      language: selectedLanguage.value,
      title: "",
      subtitle: "",
      content: "",
    };
    getLanguageMenu(data.value.contents);
  }
};

const canAddLang = computed(() => {
  return Object.keys(data.value.contents).length < LanguageTypes.all.length;
});
const updateLanguage = async (item) => {
  try {
    await GlobalService.updateEmailLanguage(data.value.id, item.id, item);
    MsgPrompt.success("Update success", item.language);
  } catch (e) {
    console.log(e);
  }
};

const createLanguage = async (item) => {
  try {
    await GlobalService.createEmailLanguage(data.value.id, item);
    MsgPrompt.success("Create success", item.language);
  } catch (e) {
    console.log(e);
  }
};

const emits = defineEmits<{
  (e: "eventSubmit"): void;
}>();

const submit = (item) => {
  isLoading.value = true;
  submitted.value = true;
  if (item.id == 0) {
    createLanguage(item);
  } else {
    updateLanguage(item);
  }
  emits("eventSubmit");
  isLoading.value = false;
  submitted.value = false;
};

defineExpose({
  show,
});
</script>
