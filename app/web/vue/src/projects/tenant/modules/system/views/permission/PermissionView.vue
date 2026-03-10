<template>
  <div class="fv-row mb-7">
    <label class="fs-6 fw-semobold mb-2">{{ $t("fields.permission") }} </label>

    <div class="row mt-5" v-for="(p, key, index) in permissions" :key="index">
      <div class="col-3">
        <label
          class="form-check form-check-sm form-check-custom form-check-solid me-5 d-inline-flex"
          ><input
            class="form-check-input"
            type="checkbox"
            v-model="checkAllKey[key]"
            @change="checkAll(key)"
            :disabled="props.disabled"
          />
          <span class="form-check-label">{{ $t("permissions." + key) }}</span>
        </label>
      </div>
      <div class="col-9">
        <div class="row">
          <!--begin::Checkbox-->
          <label
            class="form-check form-check-sm form-check-custom form-check-solid me-4 mb-3 d-inline-flex col-2"
            v-for="(val, act, index) in p"
            :key="index"
          >
            <input
              class="form-check-input"
              type="checkbox"
              name="user_management_read"
              @change="checkCheckAll(key)"
              v-model="permissions[key][act]"
              :disabled="props.disabled"
            />
            <span class="form-check-label">{{ $t("action." + act) }}</span>
          </label>
          <!--end::Checkbox-->
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
import { ref } from "vue";

const props = defineProps({
  disabled: { type: Boolean, required: false, default: false },
});

const checkAllKey = ref({});
const permissions = ref({});
const selectPermissions = ref([]);
const allPermissions = ref({});

const checkAll = (key) => {
  Object.keys(permissions.value[key]).forEach((p) => {
    permissions.value[key][p] = checkAllKey.value[key];
  });
  update();
};

const checkCheckAll = (key) => {
  let checkFalg = true;
  Object.keys(permissions.value[key]).forEach((p) => {
    if (permissions.value[key][p] == false) {
      checkFalg = false;
    }
  });
  checkAllKey.value[key] = checkFalg;
  update();
};

const initData = (data, _permissions) => {
  selectPermissions.value = data;
  allPermissions.value = _permissions;
  allPermissions.value.forEach((p) => {
    let [name, action] = p.name.split(".");
    if (action == "all") {
      return;
    }
    if (permissions.value[name] == undefined) {
      permissions.value[name] = {};
    }
    permissions.value[name][action] = data.includes(p.name);
    checkAllKey.value[name] = false;
  });
  Object.keys(checkAllKey.value).forEach((key) => {
    checkCheckAll(key);
  });
};

const update = () => {
  Object.keys(permissions.value).forEach((key) => {
    Object.keys(permissions.value[key]).forEach((act) => {
      if (permissions.value[key][act]) {
        if (!selectPermissions.value.includes(key + "." + act)) {
          selectPermissions.value.push(key + "." + act);
        }
      } else {
        var index = selectPermissions.value.indexOf(key + "." + act);
        if (index !== -1) {
          selectPermissions.value.splice(index, 1);
        }
      }
    });
  });
};

defineExpose({
  initData,
});
</script>
