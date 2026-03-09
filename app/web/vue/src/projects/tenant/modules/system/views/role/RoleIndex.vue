<template>
  <div class="" v-if="isLoading">
    {{ $t("tip.loading") }}
  </div>
  <div class="row mb-5 mb-xl-10 mx-0" v-else>
    <div class="col-md-4 mb-5" v-for="(item, index) in items" :key="index">
      <div class="card card-flush h-md-100">
        <div class="card-header">
          <div class="card-title">
            <h2>{{ item.name }}</h2>
          </div>
        </div>
        <div class="card-body pt-1">
          <div class="fw-bold text-gray-600 mb-5">
            {{ $t("tip.totalUserWithThisRole") }} {{ item.users.length }}
          </div>
        </div>
        <div class="card-footer flex-wrap pt-0">
          <button
            @click="showRole(item)"
            :id="'role_show_toggle'"
            data-bs-target="#role_show"
            type="button"
            class="btn btn-light btn-active-light-primary my-1"
          >
            {{ $t("action.viewRole") }}
          </button>
        </div>
      </div>
    </div>
    <div class="col-md-4 mb-5">
      <div class="card h-md-100">
        <div class="card-body d-flex flex-center">
          <button
            id="role_create_toggle"
            type="button"
            class="btn btn-clear d-flex flex-column flex-center"
            @click="createRole"
          >
            <img
              src="/images/illustrations/sketchy-1/4.png"
              alt=""
              class="mw-100 mh-100px mb-7"
            />
            <div class="fw-bold fs-3 text-gray-600 text-hover-primary">
              {{ $t("action.createNewRole") }}
            </div>
          </button>
        </div>
      </div>
    </div>
    <RoleCreateView
      ref="roleCreate"
      :permissions="permissions"
      @created="onCreated"
    ></RoleCreateView>
  </div>
  <RoleShowView ref="roleShow"></RoleShowView>
</template>

<script setup lang="ts">
// import ApiService from "@/core/services/ApiService";
import { ref, onMounted, inject } from "vue";
import RoleShowView from "./RoleShow.vue";
import RoleCreateView from "./RoleCreate.vue";
import ErrorMsg from "@/components/ErrorMsg";
import { apiProviderKey } from "@/core/plugins/providerKeys";

const api = inject(apiProviderKey);
const isLoading = ref(true);
const items = ref([]);
const permissions = ref([]);

const roleShow = ref(null);
const roleCreate = ref(null);
const viewData = ref({});

const fetchData = () => {
  api["role.list"]()
    .then(({ data }) => {
      items.value = data.roles;
      permissions.value = data.permissions;
      isLoading.value = false;
    })
    .catch(({ response }) => {
      console.log(response);
      ErrorMsg.show(response);
      isLoading.value = false;
    });
};

const showRole = (data) => {
  viewData.value = data;
  roleShow.value.show(data, permissions.value);
};

const createRole = () => {
  roleCreate.value.show();
};

const onCreated = (data) => {
  items.value.push(data);
};

onMounted(() => {
  fetchData();
});
</script>
