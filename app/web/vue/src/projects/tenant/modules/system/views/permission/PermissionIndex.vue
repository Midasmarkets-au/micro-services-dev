<template>
  <!--begin::Card-->
  <div class="card card-flush">
    <div class="card-header mt-6">
      <div class="card-title"></div>
      <div class="card-toolbar">
        <button
          type="button"
          class="btn btn-light-primary"
          data-bs-toggle="modal"
          data-bs-target="#permission_create"
        >
          <span class="svg-icon svg-icon-3">
            <svg
              width="24"
              height="24"
              viewBox="0 0 24 24"
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
            >
              <rect
                opacity="0.3"
                x="2"
                y="2"
                width="20"
                height="20"
                rx="5"
                fill="currentColor"
              />
              <rect
                x="10.8891"
                y="17.8033"
                width="12"
                height="2"
                rx="1"
                transform="rotate(-90 10.8891 17.8033)"
                fill="currentColor"
              />
              <rect
                x="6.01041"
                y="10.9247"
                width="12"
                height="2"
                rx="1"
                fill="currentColor"
              />
            </svg>
          </span>
          {{ $t("action.add_permission") }}
        </button>
      </div>
    </div>
    <div class="card-body pt-0" v-if="!isLoading">
      <table
        class="table align-middle table-row-dashed fs-6 gy-5 mb-0"
        id="kt_permissions_table"
      >
        <thead>
          <tr
            class="text-start text-mutedtext-gray-400 fw-bold fs-7 text-uppercase gs-0"
          >
            <th class="min-w-125px">{{ $t("fields.name") }}</th>
            <th class="min-w-250px">{{ $t("permissions.assignedto") }}</th>
            <th class="text-end min-w-100px">{{ $t("fields.actions") }}</th>
          </tr>
        </thead>
        <tbody class="fw-semibold text-gray-900">
          <tr v-for="(actions, permission, index) in permissions" :key="index">
            <td>{{ $t("permissions." + permission) }}</td>
            <td>
              <!-- <a
                href="../../demo1/dist/apps/user-management/roles/view.html"
                class="badge badge-light-primary fs-7 m-1"
                >Administrator</a
              > -->
            </td>
            <td class="text-end">
              <template v-for="(act, idx) in actions" :key="idx">
                {{ $t("action." + act) }},&nbsp;
              </template>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <div class="card-body pt-0" v-else>
      {{ $t("tip.loading") }}
    </div>
  </div>
  <CreateForm></CreateForm>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import ApiService from "@/core/services/ApiService";
import ErrorMsg from "@/components/ErrorMsg";
import CreateForm from "./PermissionCreate.vue";

export default defineComponent({
  name: "system-permissions",
  components: {
    CreateForm,
  },
  data() {
    return {
      isLoading: true,
      permissions: {},
    };
  },
  mounted() {
    this.fetchData();
  },
  methods: {
    fetchData() {
      ApiService.get("permissions")
        .then(({ data }) => {
          this.initData(data.permissions);
          this.isLoading = false;
        })
        .catch(({ response }) => {
          ErrorMsg.show(response);
          this.isLoading = false;
        });
      console.log("fetch data");
    },
    initData(permissions) {
      for (let permission of permissions) {
        let [name, action] = permission.name.split(".");
        if (this.permissions[name] == undefined) {
          this.permissions[name] = [] as string[];
        }
        (this.permissions[name] as unknown as string[]).push(action);
      }
    },
  },
});
</script>
