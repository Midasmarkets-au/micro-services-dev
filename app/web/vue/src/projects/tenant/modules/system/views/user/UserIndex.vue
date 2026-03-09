<template>
  <div class="d-flex flex-column flex-column-fluid">
    <div id="kt_app_content" class="app-content flex-column-fluid">
      <div id="kt_app_content_container" class="app-container container-xxl">
        <div class="card">
          <div class="card-header border-0 pt-6">
            <div class="card-title">
              <div class="d-flex align-items-center position-relative my-1">
                <input
                  type="text"
                  data-kt-user-table-filter="search"
                  class="form-control form-control-solid w-250px ps-14"
                  placeholder="Search user"
                />
              </div>
            </div>
            <div class="card-toolbar">
              <div
                class="d-flex justify-content-end"
                data-kt-user-table-toolbar="base"
              >
                <button
                  id="user_create_toggle"
                  type="button"
                  class="btn btn-primary"
                  @click="createUser"
                >
                  Add User
                </button>
              </div>
            </div>
          </div>
          <div class="card-body py-4">
            <table
              class="table align-middle table-row-dashed fs-6 gy-5"
              id="kt_table_users"
            >
              <thead>
                <tr
                  class="text-start text-muted fw-bold fs-7 text-uppercase gs-0"
                >
                  <th class="w-10px pe-2">
                    <div
                      class="form-check form-check-sm form-check-custom form-check-solid me-3"
                    >
                      <input
                        class="form-check-input"
                        type="checkbox"
                        data-kt-check="true"
                        data-kt-check-target="#kt_table_users .form-check-input"
                        value="1"
                      />
                    </div>
                  </th>
                  <th class="min-w-125px">User</th>
                  <th class="min-w-125px">Role</th>
                  <th class="min-w-125px">Lang</th>
                  <th class="min-w-125px">Timezone</th>
                  <th class="min-w-125px text-center">Two-step</th>
                  <th class="min-w-125px">Last login</th>
                  <th class="text-center min-w-100px">Actions</th>
                </tr>
              </thead>

              <tbody v-if="isLoading">
                <LoadingRing />
              </tbody>
              <tbody v-else-if="!isLoading && items.length === 0">
                <NoDataBox />
              </tbody>

              <tbody v-else class="fw-semibold text-gray-900">
                <tr v-for="(item, index) in items" :key="index">
                  <td>
                    <div
                      class="form-check form-check-sm form-check-custom form-check-solid"
                    >
                      <input
                        class="form-check-input"
                        type="checkbox"
                        value="1"
                      />
                    </div>
                  </td>
                  <td class="d-flex align-items-center">
                    <div
                      class="symbol symbol-circle symbol-50px overflow-hidden me-3"
                    >
                      <a
                        href="../../demo1/dist/apps/user-management/users/view.html"
                      >
                        <div class="symbol-label">
                          <AuthImage
                            class="w-100"
                            :imageGuid="item.avatar"
                            :alt="item.name"
                          />
                        </div>
                      </a>
                    </div>
                    <div class="d-flex flex-column">
                      <a
                        href="../../demo1/dist/apps/user-management/users/view.html"
                        class="text-gray-800 text-hover-primary mb-1"
                        >{{ item.name }}</a
                      >
                      <span>{{ item.email }}</span>
                    </div>
                  </td>
                  <td>Administrator</td>
                  <td>
                    <div class="badge badge-light fw-bold">{{ item.lang }}</div>
                  </td>
                  <td>{{ item.timezone }}</td>
                  <td class="text-center">ON</td>
                  <td>09/28 10:00</td>
                  <td class="text-center">
                    <a
                      href="#"
                      class="btn btn-light btn-active-light-primary btn-sm"
                      data-kt-menu-trigger="click"
                      data-kt-menu-placement="bottom-end"
                      >Actions</a
                    >
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  </div>

  <UserCreateView ref="userCreate" @created="onCreated"></UserCreateView>
</template>

<script setup lang="ts">
import { ref, onMounted, inject } from "vue";
import ErrorMsg from "@/components/ErrorMsg";
import UserCreateView from "./UserCreate.vue";
import AuthImage from "@/components/AuthImage.vue";
import { apiProviderKey } from "@/core/plugins/providerKeys";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";

const api = inject(apiProviderKey);
const isLoading = ref(true);
const items = ref([]);
const userCreate = ref(null);
const pagenation = ref([]);

const fetchData = () => {
  api["user.list"]()
    .then(({ data }) => {
      items.value = data.data;
      console.log("pagenation", pagenation);
      pagenation.value = data.links;
      console.log("pagenation", pagenation.value.first);
      isLoading.value = false;
    })
    .catch(({ response }) => {
      console.log(response);
      ErrorMsg.show(response);
      isLoading.value = false;
    });
  console.log("fetch data");
};

onMounted(() => {
  fetchData();
});

const createUser = () => {
  userCreate.value.show();
};

const onCreated = (data) => {
  items.value.unshift(data);
};
</script>
