const routes = [
  {
    path: "/",
    component: () => import("layouts/MainLayout.vue"),
    children: [
      { path: "/", component: () => import("pages/IndexPage.vue") },
      { path: "/stations", component: () => import("pages/StationsPage.vue") },
      {
        path: "/stations/:id(\\d+)",
        component: () => import("pages/StationPage.vue"),
      },
      { path: "/rides", component: () => import("pages/RidesPage.vue") },
      {
        path: "/cities/:id(\\d+)",
        component: () => import("pages/CityPage.vue"),
      },
      { path: "/cities", component: () => import("pages/CitiesPage.vue") },
      {
        path: "/dev-extras",
        component: () => import("pages/DevExtrasPage.vue"),
      },
    ],
  },

  // Always leave this as last one,
  // but you can also remove it
  {
    path: "/:catchAll(.*)*",
    component: () => import("pages/ErrorNotFound.vue"),
  },
];

export default routes;
