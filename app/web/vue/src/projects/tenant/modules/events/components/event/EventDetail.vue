<template>
  <el-dialog
    v-model="dialogRef"
    :title="title"
    width="1200"
    align-center
    class="rounded"
  >
    <el-form
      :model="formData"
      :rules="formRules"
      ref="ruleFormRef"
      label-position="top"
    >
      <el-form-item label="Title" prop="title">
        <el-input v-model="formData.title" placeholder="Title"></el-input>
      </el-form-item>
      <div class="d-flex gap-18">
        <el-form-item label="Event Start Time" prop="startTime">
          <el-date-picker
            v-model="formData.startTime"
            type="datetime"
            format="YYYY/MM/DD HH:mm:ss"
            class="w-200px"
          />
        </el-form-item>
        <el-form-item label="Event End Time" prop="endTime">
          <el-date-picker
            v-model="formData.endTime"
            type="datetime"
            format="YYYY/MM/DD HH:mm:ss"
            class="w-200px"
          />
        </el-form-item>
      </div>
      <div class="d-flex gap-15">
        <el-form-item label="Service" prop="serviceId">
          <el-select
            v-model="formData.serviceId"
            placeholder="Service"
            class="w-200px"
          >
            <el-option
              v-for="item in servicesOptions"
              :key="item.id"
              :label="item.description"
              :value="item.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="Published" prop="isPublished" class="ms-3">
          <el-switch
            v-model="formData.isPublished"
            active-color="#13ce66"
            inactive-color="#ff4949"
          />
        </el-form-item>
      </div>
      <div class="d-flex gap-15">
        <el-form-item label="Account Types" prop="accountTypes">
          <el-checkbox-group v-model="formData.accountTypes">
            <el-checkbox
              v-for="item in ConfigEventAccountTypeSelections"
              :key="item.value"
              :label="item.value"
            >
              {{ item.label }}
            </el-checkbox>
          </el-checkbox-group>
        </el-form-item>
        <el-form-item label="Reward Paths" prop="path">
          <el-checkbox-group v-model="formData.path">
            <el-checkbox
              v-for="item in rewardPathSelections"
              :key="item.value"
              :label="item.value"
            >
              {{ item.label }}
            </el-checkbox>
          </el-checkbox-group>
        </el-form-item>
      </div>
    </el-form>
    <div>
      <p class="fs-6">{{ $t("fields.content") }}</p>
      <div class="input-group w-md-25 mb-4">
        <select class="form-select" v-model="langItem" :disabled="!canAddLang">
          <option
            v-for="item in langOptions"
            :key="item.code"
            :value="item.code"
            :label="item.code + '   (' + item.name + ')'"
          />
        </select>
        <button
          class="btn btn-success"
          @click="addLang"
          :disabled="!canAddLang"
        >
          {{ $t("action.addALanguage") }}
        </button>
      </div>
      <el-tabs>
        <el-tab-pane
          :label="index"
          v-for="(item, index) in contentList"
          :key="index"
        >
          <div>
            <quill-editor
              :key="index"
              theme="snow"
              v-model:content="contentList[index]"
              :modules="getModules(index)"
              contentType="html"
              toolbar="full"
            />
          </div>
        </el-tab-pane>
      </el-tabs>
    </div>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false">{{
          $t("action.cancel")
        }}</el-button>
        <el-button type="primary" @click="submit">
          {{ $t("action.submit") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script setup lang="ts">
import { ref, reactive, onMounted, computed } from "vue";
import EventsServices from "../../services/EventsServices";
import { type FormRules, FormInstance } from "element-plus";
import { ConfigEventAccountTypeSelections } from "@/core/types/AccountInfos";
import { QuillEditor } from "@vueup/vue-quill";
import "@vueup/vue-quill/dist/vue-quill.snow.css";
import ImageUploader from "quill-image-uploader";
import BlotFormatter from "quill-blot-formatter/dist/BlotFormatter";
import { LanguageTypes } from "@/core/types/LanguageTypes";

const dialogRef = ref(false);
const title = ref("");
const eventType = ref("");
const isLoading = ref(false);
const formData = ref<any>({});
const ruleFormRef = ref<FormInstance>();
const langOptions = ref<any>([]);
const langItem = ref<any>("");
const contentList = ref({});
const servicesOptions = ref([]);
const rewardPathSelections = ref([
  {
    value: "Commission",
    label: "Commission",
  },
  {
    value: "Items",
    label: "Items",
  },
]);

const formRules = reactive<FormRules>({
  title: [{ required: true, message: "Please input title", trigger: "blur" }],
  accountTypes: [
    {
      required: true,
      message: "Please select at least one account type",
      trigger: "blur",
    },
  ],
  path: [
    {
      required: true,
      message: "Please select at least one reward path",
      trigger: "blur",
    },
  ],
  serviceId: [
    { required: true, message: "Please select a service", trigger: "blur" },
  ],
  isPublished: [
    {
      required: true,
      message: "Please select a published status",
      trigger: "blur",
    },
  ],
});

const show = (type: string, item?: any) => {
  eventType.value = type;
  if (type === "edit") {
    title.value = item.title;
  } else {
    title.value = "Create Event";
  }
  dialogRef.value = true;
};

const hide = () => {
  dialogRef.value = false;
};

const populateLangOptions = () => {
  const langKeys = Object.keys(contentList.value);
  langOptions.value = LanguageTypes.all.filter(
    (lang) => !langKeys.includes(lang.code)
  );

  if (langOptions.value.length > 0) {
    langItem.value = langOptions.value[0].code;
  }
  // if (langOptions.value.length === LanguageTypes.all.length) {
  //   addLang();
  // }
};

const addLang = () => {
  if (Object.keys(contentList.value).length < LanguageTypes.all.length) {
    contentList.value[langItem.value] = "content " + langItem.value;
    populateLangOptions();
  }
};

const canAddLang = computed(() => {
  return Object.keys(contentList.value).length < LanguageTypes.all.length;
});

const uploadImg = async (file: any) => {
  const formData = new FormData();
  formData.append("file", file);
  const res = await EventsServices.uploadImage(formData);
  return res["url"];
};

const submit = async () => {
  console.log(formData.value);
  // if (!ruleFormRef.value) return;

  // let isValid = false;
  // await ruleFormRef.value.validate(async (valid, fields) => {
  //   isValid = valid;
  // });
  // if (!isValid) return;
};

const getModules = (index: number) => {
  return [
    {
      name: "imageUploader" + index,
      module: ImageUploader,
      options: {
        upload: (file) => {
          return uploadImg(file);
        },
      },
    },
    {
      name: "blotFormatter" + index,
      module: BlotFormatter,
    },
  ];
};

onMounted(async () => {
  populateLangOptions();
  servicesOptions.value = await EventsServices.getServices();
});
defineExpose({
  show,
  hide,
});
</script>
<style scoped>
:deep .ql-container {
  min-height: 450px;
}
</style>
