<template>
  <el-dialog
    v-model="dialogRef"
    :title="$t('title.adjustPoints')"
    width="500"
    align-center
  >
    <div>
      <el-form
        ref="ruleFormRef"
        :model="form"
        :rules="rules"
        label-width="auto"
      >
        <el-form-item :label="t('fields.points')" prop="point">
          <el-input-number
            v-model="form.point"
            :disabled="isLoading"
          ></el-input-number>
        </el-form-item>
        <el-form-item :label="t('fields.reason')" prop="comment">
          <el-input v-model="form.comment" :disabled="isLoading"></el-input>
        </el-form-item>
      </el-form>
    </div>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false" :disabled="isLoading">{{
          $t("action.cancel")
        }}</el-button>
        <el-button type="primary" @click="submit" :loading="isLoading">
          {{ $t("action.submit") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref, reactive } from "vue";
import EventsServices from "../../../services/EventsServices";
import type { FormInstance } from "element-plus";
import Decimal from "decimal.js";
import { ElNotification } from "element-plus";
import i18n from "@/core/plugins/i18n";

const t = i18n.global.t;
const ruleFormRef = ref<FormInstance>();
const dialogRef = ref(false);
const isLoading = ref(false);
const eventPartyId = ref(0);
const form = ref({
  point: 0,
  comment: "",
});

function validatePoint(rule, value, callback) {
  if (value == 0) {
    callback(new Error(t("error.adjustPointError")));
  } else {
    callback();
  }
}
const rules = reactive<any>({
  point: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "blur" },
    { validator: validatePoint, trigger: "blur" },
  ],
  comment: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "blur" },
  ],
});

const emits = defineEmits(["update"]);

const submit = async () => {
  if (!ruleFormRef.value) return;

  let isValid = false;
  await ruleFormRef.value.validate(async (valid, fields) => {
    isValid = valid;
  });
  if (!isValid) return;
  isLoading.value = true;
  try {
    await EventsServices.adjustPoints({
      eventPartyId: eventPartyId.value,
      point: new Decimal(form.value.point).mul(10000).toNumber(),
      comment: form.value.comment,
    });
    emits("update");
    form.value = {
      point: 0,
      comment: "",
    };
    dialogRef.value = false;
    ElNotification({
      title: t("status.success"),
      type: "success",
    });
  } catch (error) {
    console.error(error);
    ElNotification({
      title: t("status.error"),
      type: "error",
    });
  }
  isLoading.value = false;
};

const show = (item) => {
  dialogRef.value = true;
  eventPartyId.value = item.id;
};

defineExpose({
  show,
});
</script>
