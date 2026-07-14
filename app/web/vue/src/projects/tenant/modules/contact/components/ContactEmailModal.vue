<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('action.sendEmail')"
    :is-loading="isLoading"
    :submit="submit"
    :width="760"
    :disable-submit="isLoading"
  >
    <div class="mb-5 text-gray-700">
      {{ $t("fields.receiverEmail") }}:
      <span class="fw-bold">{{ contact.email }}</span>
    </div>

    <el-form label-position="top">
      <el-form-item :label="$t('fields.title')" required>
        <el-input v-model="form.title" />
      </el-form-item>
      <el-form-item :label="$t('fields.subtitle')" required>
        <el-input v-model="form.subtitle" />
      </el-form-item>
      <el-form-item :label="$t('fields.language')">
        <el-select v-model="form.language" class="w-100">
          <el-option
            v-for="language in LanguageTypes.all"
            :key="language.code"
            :label="language.englishName || language.name"
            :value="language.code"
          />
        </el-select>
      </el-form-item>
      <el-form-item :label="$t('fields.content')" required>
        <el-input v-model="form.content" type="textarea" :rows="10" />
      </el-form-item>
    </el-form>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { LanguageCodes, LanguageTypes } from "@/core/types/LanguageTypes";
import ContactService from "../services/ContactService";

const emits = defineEmits<{
  (event: "sent"): void;
}>();

const modalRef = ref<any>();
const isLoading = ref(false);
const contact = ref<any>({});
const form = ref({
  title: "",
  subtitle: "",
  content: "",
  language: LanguageCodes.enUS,
});

const show = (_contact: any) => {
  contact.value = _contact;
  form.value = {
    title: "",
    subtitle: "",
    content: "",
    language: LanguageCodes.enUS,
  };
  modalRef.value?.show();
};

const submit = async () => {
  if (!form.value.title || !form.value.subtitle || !form.value.content) {
    MsgPrompt.warning("Please fill title, subtitle and content.");
    return;
  }

  isLoading.value = true;
  try {
    await ContactService.sendContactEmail(contact.value.id, form.value);
    MsgPrompt.success();
    emits("sent");
    modalRef.value?.hide();
  } catch (e) {
    MsgPrompt.error(e);
  } finally {
    isLoading.value = false;
  }
};

defineExpose({
  show,
  hide: () => modalRef.value?.hide(),
});
</script>
