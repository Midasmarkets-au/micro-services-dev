<template>
  <SiderDetail
    :save="submit"
    :discard="close"
    :title="detailTitle"
    elId="user_detail_show"
    :isLoading="isLoading"
    :submited="submited"
    :isDisabled="false"
    :savingTitle="$t('action.saving')"
    :show-footer="false"
    width="{default:'90%'}"
    ref="emailDetailShowRef"
  >
    <div v-if="!isLoading">
      <div class="fv-row mb-7">
        <ul
          class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
        >
          <li
            class="nav-item"
            v-for="(item, key, index) in detail.contents"
            :key="index"
          >
            <button
              class="nav-link text-active-primary pb-4"
              :class="{ active: tab === key }"
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
      </div>
      <div class="row">
        <div class="col-6">
          <el-form
            ref="emailLangFormRef"
            :model="langDetail"
            :rules="langFormRules"
            label-width="120px"
            class="demo-ruleForm"
            size="default"
            status-icon
          >
            <el-form-item :label="$t('fields.language')" prop="role">
              <el-select v-model="langDetail.language">
                <el-option
                  v-for="item in languageOptions"
                  :key="item.code"
                  :label="item.name"
                  :value="item.code"
                />
              </el-select>
            </el-form-item>

            <el-form-item :label="$t('fields.title')" prop="title">
              <el-input v-model="langDetail.title" />
            </el-form-item>

            <el-form-item :label="$t('fields.subtitle')" prop="subtitle">
              <el-input v-model="langDetail.subtitle" />
            </el-form-item>
            <el-form-item :label="$t('fields.content')" prop="title">
              <el-input
                v-model="langDetail.content"
                type="textarea"
                @change="refreshLayoutTemplateHtml"
                :autosize="{ minRows: 20 }"
              />
            </el-form-item>

            <div class="text-end">
              <button
                class="btn btn-warning me-3"
                @click.prevent="sendTest(detail.title, langDetail.language)"
              >
                Test
              </button>
              <button :class="`btn btn-secondary me-3`" @click.prevent="close">
                {{ $t("action.close") }}
              </button>
              <button
                :class="`btn btn-primary me-3`"
                type="submit"
                :disabled="submited || isLoading"
                @click.prevent="submit"
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
        <div class="col-6">
          <iframe
            style="width: 600px; height: 100%"
            ref="templateHtmlShowRef"
            :srcdoc="layoutTemplateHtml"
          >
          </iframe>
        </div>
      </div>
    </div>
  </SiderDetail>
  <SendTestEmail ref="sendTestEmailRef" />
</template>
<script setup lang="ts">
import { ref, reactive, computed, onMounted } from "vue";
import SiderDetail from "@/components/SiderDetail.vue";
import { LanguageTypes } from "@/core/types/LanguageTypes";
import GlobalService from "../../../services/TenantGlobalService";
import type { FormInstance, FormRules } from "element-plus";
import SendTestEmail from "./SendTestEmail.vue";

const emailDetailShowRef = ref<InstanceType<typeof SiderDetail>>();
const sendTestEmailRef = ref<InstanceType<typeof SendTestEmail>>();
const templateHtmlShowRef = ref(null);

const isLoading = ref(true);
const submited = ref(false);

const detail = ref(Array<any>());
const detailTitle = ref("");

const layoutTemplate = ref({});

const emailLangFormRef = ref<FormInstance>();

const languageOptions = ref([]);

const langDetail = ref({
  title: "",
  subtitle: "",
  content: "",
  language: "",
  id: 0,
});

const isAllLanguage = ref(false);

const langFormRules = reactive<FormRules>({
  title: [{ required: true, message: "Please input title" }],
  subtitle: [{ required: true, message: "Please input subtitle" }],
  content: [{ required: true, message: "Please input content" }],
  language: [{ required: true, message: "Please select language" }],
});

const layoutTemplateHtml = ref("");

const refreshLayoutTemplateHtml = () => {
  if (templateHtmlShowRef.value == null) {
    return;
  }
  if (langDetail.value.language == "") {
    layoutTemplateHtml.value = "";
    templateHtmlShowRef.value.srcdoc = "";
    return;
  }
  if (detail.value.id == layoutTemplate.value.id) {
    layoutTemplateHtml.value =
      layoutTemplate.value.contents[langDetail.value.language].content;
    templateHtmlShowRef.value.srcdoc = layoutTemplateHtml.value;
    return;
  }
  var t = layoutTemplate.value.contents[langDetail.value.language].content;
  t = t.replace("{{Title}}", langDetail.value.title);
  t = t.replace("{{Subtitle}}", langDetail.value.subtitle);
  t = t.replace("{{Content}}", langDetail.value.content);
  layoutTemplateHtml.value = t;
  templateHtmlShowRef.value.srcdoc = t;
  return;
};

const tab = ref("new");

const show = (data, layout) => {
  layoutTemplate.value = layout;
  detail.value = data;
  detailTitle.value = detail.value.title + " Email Detail";

  let keys = Object.keys(detail.value.contents);
  if (keys.length > 0) {
    changeTab(keys[0], detail.value.contents[keys[0]]);
  } else {
    changeTab("new", null);
  }

  isAllLanguage.value = keys.length >= LanguageTypes.all.length;
  isLoading.value = false;
  emailDetailShowRef.value?.show();
  refreshLayoutTemplateHtml();
};

const sendTest = (title, lang) => {
  sendTestEmailRef.value.show(title, lang);
};

const changeTab = (_tab, item) => {
  tab.value = _tab;
  if (item) {
    langDetail.value = {
      title: item.title,
      subtitle: item.subtitle,
      content: item.content,
      language: item.language,
      id: item.id,
    };
    languageOptions.value = LanguageTypes.all.filter(
      (langItem) => langItem.code == item.language
    );
  } else {
    langDetail.value = {
      title: "",
      subtitle: "",
      content: "",
      language: "",
      id: 0,
    };
    languageOptions.value = LanguageTypes.all.filter(
      (langItem) => detail.value.contents[langItem.code] == undefined
    );
  }
  refreshLayoutTemplateHtml();
};

const emits = defineEmits<{
  (e: "eventSubmit"): void;
}>();

const submit = async () => {
  submited.value = true;
  console.log(langDetail.value);
  if (langDetail.value.id == 0) {
    await createEmailLanguage();
  } else {
    await updateEmailLanguage();
  }
  emits("eventSubmit");
  refreshLayoutTemplateHtml();
  submited.value = false;
};

const updateEmailLanguage = async () => {
  console.log(langDetail.value.id);
  GlobalService.updateEmailLanguage(
    detail.value.id,
    langDetail.value.id,
    langDetail.value
  );
  console.log("update email language");
};

const createEmailLanguage = async () => {
  GlobalService.createEmailLanguage(detail.value.id, langDetail.value);
  console.log("save email language");
};
const close = () => {
  emailDetailShowRef.value?.hide();
  emits("eventSubmit");
};
onMounted(() => {
  //
});

defineExpose({ show });
</script>
