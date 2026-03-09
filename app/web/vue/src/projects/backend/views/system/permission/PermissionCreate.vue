<template>
  <ModelForm
    :submit="submit"
    :discard="close"
    :title="$t('action.create')"
    elId="permission_create"
    :isLoading="isLoading"
    :submited="submited"
    :savingTitle="$t('action.saving')"
    ref="ref_permission_create_model"
    :rules="rules"
    :formData="formData"
  >
    <div class="fv-row mb-7">
      <label class="required fs-6 fw-semobold mb-2">{{
        $t("fields.name")
      }}</label>
      <el-form-item prop="name">
        <el-input
          v-model="formData.name"
          name="name"
          type="text"
          placeholder=""
        />
      </el-form-item>
    </div>

    <div class="mb-15 fv-row">
      <label class="required fs-6 fw-semobold mb-2">
        {{ $t("action.action") }}
      </label>
      <div class="d-flex align-items-center row">
        <el-checkbox
          v-model="checkAll"
          class="col-3"
          :indeterminate="isIndeterminate"
          @change="handleCheckAllChange"
          >{{ $t("action.all") }}
        </el-checkbox>
        <el-checkbox-group
          class="col-9"
          v-model="formData.actions"
          @change="handleCheckedActionsChange"
        >
          <div class="row">
            <el-checkbox
              class="col-2"
              v-for="act in permissionActions"
              :key="act"
              :label="act"
              >{{ act }}</el-checkbox
            >
          </div>
        </el-checkbox-group>
      </div>
    </div>
  </ModelForm>
</template>
<script lang="ts">
import { defineComponent, ref } from "vue";
import ModelForm from "@/components/ModelForm.vue";
import i18n from "@/core/plugins/i18n";

export default defineComponent({
  name: "system-permission-create",
  components: {
    ModelForm,
  },
  data() {
    return {
      isLoading: false,
      submited: false,
      // ref_permission_create: ref<null | HTMLElement>(null),
      checkAll: ref(false),
      isIndeterminate: ref(true),
      permissionActions: [
        i18n.global.t("actionlist"),
        i18n.global.t("actionread"),
        i18n.global.t("actionupdate"),
        i18n.global.t("actiondelete"),
        i18n.global.t("actionassign"),
        i18n.global.t("actionrevoke"),
        i18n.global.t("actioncreate"),
        // i18n.global.t("actionall"),
      ],
      formData: ref({
        name: "",
        actions: [] as string[],
      }),
      rules: ref({
        name: [
          {
            required: true,
            message: i18n.global.t("permissions.create.nameRequire"),
            trigger: "change",
          },
        ],
        actions: [
          {
            required: true,
            message: i18n.global.t("permissions.create.action_require"),
            trigger: "change",
            type: "array",
            fields: {
              0: { type: "string", required: true },
            },
          },
        ],
      }),
    };
  },
  mounted() {
    // this.formRef_permission_create = ref<null | HTMLFormElement>(null);
    // this.ref_permission_create = ref<null | HTMLElement>(null);
  },
  methods: {
    submit() {
      // console.log(this.formRef.value);
      // validate((valid) => {
      //   if (valid) {
      //     loading.value = true;
      console.log(this.formData);
    },
    close() {
      // this.$refs["ref_permission_create_model"].hide();
    },
    handleCheckAllChange(val: boolean) {
      this.formData.actions = val ? this.permissionActions : ([] as string[]);
      console.log(val, this.formData.actions);
      this.isIndeterminate = false;
    },
    handleCheckedActionsChange(value: string[]) {
      console.log(value);
      const checkedCount = value.length;
      this.checkAll = checkedCount === this.permissionActions.length;
      this.isIndeterminate =
        checkedCount > 0 && checkedCount < this.permissionActions.length;
    },
  },
});
</script>
